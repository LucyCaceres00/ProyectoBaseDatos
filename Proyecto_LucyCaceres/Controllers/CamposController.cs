using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using Proyecto_LucyCaceres.Models;
namespace Proyecto_LucyCaceres.Controllers
{
    public class CamposController : ApiController
    {
        private SqlDatabaseEntities sql = new SqlDatabaseEntities();
        private MySqlDatabaseEntities mySql = new MySqlDatabaseEntities();

        [HttpGet]
        [Route("api/campos/getDataType")]
        [ResponseType(typeof(CamposVM))]
        public IHttpActionResult getTablasSQL()
        {
            List<CamposVM> tablas = new List<CamposVM>();
            tablas = sql.Database.SqlQuery<CamposVM>(@"
                        SELECT 
                            name AS tipoDato
                        FROM 
                            sys.types").ToList(); ;

            if (tablas.Count == 0)
            {
                return NotFound();
            }

            return Ok(tablas);
        }

        [HttpGet]
        [Route("api/listadoCampos/{nombreTabla}")]
        [ResponseType(typeof(CamposVM))]
        public IHttpActionResult GetDetallesCampos(string nombreTabla)
        {
            List<CamposVM> detallesCampos = new List<CamposVM>();

            detallesCampos = sql.Database.SqlQuery<CamposVM>(@"
                            SELECT 
                                c.name AS nombre,
                                t.name AS tipoDato,
                                c.is_nullable AS isNull,
                                CASE 
                                    WHEN pk.column_id IS NOT NULL THEN 1
                                    ELSE 0
                                END AS isPrimaryKey
                            FROM 
                                sys.columns c
                            INNER JOIN 
                                sys.types t ON c.user_type_id = t.user_type_id
                            LEFT JOIN 
                                (SELECT i.object_id, ic.column_id 
                                 FROM sys.indexes i 
                                 INNER JOIN sys.index_columns ic ON i.index_id = ic.index_id AND i.object_id = ic.object_id 
                                 WHERE i.is_primary_key = 1) pk ON c.object_id = pk.object_id AND c.column_id = pk.column_id
                            WHERE 
                                c.object_id = OBJECT_ID(@p0)", nombreTabla).ToList();

            if (detallesCampos.Count == 0)
            {
                return NotFound();
            }

            return Ok(detallesCampos);
        }

        [HttpPut]
        [Route("api/eliminarColumna/{nombreTabla}/{nombreColumna}")]
        [ResponseType(typeof(CamposVM))]
        public IHttpActionResult eliminarColumna(string nombreTabla, string nombreColumna)
        {
            try
            {
                var referencias = sql.Database.SqlQuery<int>(@"
                                    SELECT COUNT(*)
                                    FROM sys.foreign_key_columns AS fkc
                                    INNER JOIN sys.columns AS c
                                        ON fkc.parent_object_id = c.object_id AND fkc.parent_column_id = c.column_id
                                    WHERE c.name = @p0 AND OBJECT_NAME(fkc.parent_object_id) = @p1",
                                    nombreColumna, nombreTabla).Single();

                if (referencias > 0)
                {
                    return Content(HttpStatusCode.BadRequest, new { message = $"La columna '{nombreColumna}' no se puede eliminar, ya que otras tablas tienen dependencia de esta." });
                }

                sql.Database.ExecuteSqlCommand($"ALTER TABLE {nombreTabla} DROP COLUMN {nombreColumna}");

                return Content(HttpStatusCode.OK, new { message = $"La columna '{nombreColumna}' ha sido eliminada exitosamente." });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("api/agregarColumna/{nombreTabla}")]
        public IHttpActionResult AgregarColumnaSql(string nombreTabla, [FromBody] CamposVM columna)
        {
            try
            {
                if (string.IsNullOrEmpty(columna.nombre) || string.IsNullOrEmpty(columna.tipoDato))
                {
                    return Content(HttpStatusCode.BadRequest, new { message = $"El nombre del campo y el tipo de dato son obligatorios."});
                }

                //Validacion para no agregar no columnas nulas en una tabla con registros.
                string query = $@" SELECT COUNT(*) FROM {nombreTabla}";
                var campo = sql.Database.SqlQuery<int>(query).Single();
                if (campo > 0 && !columna.isNull)
                {
                    return Content(HttpStatusCode.BadRequest, new { message = $"El campo no se puede crear de tipo opcional debido a que ya existen registros en la tabla seleccionada." });
                }

                //Validacion para verificar que no exista columna con el mismo nombre
                string getColumnaQuery = $@"
                        SELECT COUNT(*)
                        FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_NAME = '{nombreTabla}'
                          AND COLUMN_NAME = '{columna.nombre}'";

                var columnaExiste = sql.Database.SqlQuery<int>(getColumnaQuery).Single();

                if (columnaExiste > 0)
                {
                    return Content(HttpStatusCode.BadRequest, new { message = $"Ya existe un campo con ese nombre en la tabla seleccionada."});
                }

                string tipoDatoCompleto = columna.tipoDato;
                if (!string.IsNullOrEmpty(columna.especificacion))
                {
                    tipoDatoCompleto += $"({columna.especificacion})";
                }

                string checkPrimaryKeyQuery = $@"
                                                SELECT COUNT(*)
                                                FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
                                                WHERE TABLE_NAME = '{nombreTabla}'
                                                  AND CONSTRAINT_TYPE = 'PRIMARY KEY'";

                var primaryKeyExists = sql.Database.SqlQuery<int>(checkPrimaryKeyQuery).Single();

                if (primaryKeyExists > 0 && columna.primaryKey)
                {
                    return Content(HttpStatusCode.BadRequest, new { message = $"La tabla ya tiene una clave primaria."});
                }

                string isNullClause = columna.isNull ? "NULL" : "NOT NULL";

                string sqlCommand = $@"
                    ALTER TABLE {nombreTabla}
                    ADD {columna.nombre} {tipoDatoCompleto} {isNullClause}";
                sql.Database.ExecuteSqlCommand(sqlCommand);
                if (columna.primaryKey)
                {
                    string primaryKeyCommand = $@"
                    ALTER TABLE {nombreTabla}
                    ADD CONSTRAINT PK_{nombreTabla}_{columna.nombre} PRIMARY KEY ({columna.nombre})";
                    sql.Database.ExecuteSqlCommand(primaryKeyCommand);
                }
                return Content(HttpStatusCode.OK, new { message = $"La columna '{columna.nombre}' ha sido creada exitosamente." });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("api/editarColumna/{nombreTabla}/{nombreCampo}")]
        public IHttpActionResult EditarColumnaSql(string nombreTabla, string nombreCampo, [FromBody] CamposVM columna)
        {
            try
            {
                if (string.IsNullOrEmpty(columna.nombre) || string.IsNullOrEmpty(columna.tipoDato))
                {
                    return Content(HttpStatusCode.BadRequest, new { message = $"El nombre del campo y el tipo de dato son obligatorios." });
                }

                //erificar si el nuevo nombre de columna ya existe (si el nombre será cambiado)
                if (!string.Equals(nombreCampo, columna.nombre, StringComparison.OrdinalIgnoreCase))
                {
                    string queryExisteColumna = $@"
                            SELECT COUNT(*)
                            FROM INFORMATION_SCHEMA.COLUMNS
                            WHERE TABLE_NAME = '{nombreTabla}'
                              AND COLUMN_NAME = '{columna.nombre}'";
                    var existeColumna = sql.Database.SqlQuery<int>(queryExisteColumna).Single();

                    if (existeColumna > 0)
                    {
                        return Content(HttpStatusCode.BadRequest, new { message = $"Ya existe un campo con el nombre '{columna.nombre}' en la tabla seleccionada." });
                    }
                }

                // Si la tabla tiene registros y la columna será NOT NULL
                if (!columna.isNull)
                {
                    string queryHayDatos = $@"SELECT COUNT(*) FROM {nombreTabla}";
                    var hayDatos = sql.Database.SqlQuery<int>(queryHayDatos).Single();
                    if (hayDatos > 0)
                    {
                        return Content(HttpStatusCode.BadRequest, new { message = $"La columna no puede cambiarse a 'NOT NULL' porque la tabla tiene registros existentes." });
                    }
                }

                string tipoDatoCompleto = columna.tipoDato;
                if (!string.IsNullOrEmpty(columna.especificacion))
                {
                    tipoDatoCompleto += $"({columna.especificacion})";
                }
                string isNullTipe = columna.isNull ? "NULL" : "NOT NULL";

                string alterColumn = $@"
                        ALTER TABLE {nombreTabla}
                        ALTER COLUMN {nombreCampo} {tipoDatoCompleto} {isNullTipe}";
                sql.Database.ExecuteSqlCommand(alterColumn);

                // Renombrar columna
                if (!string.Equals(nombreCampo, columna.nombre, StringComparison.OrdinalIgnoreCase))
                {
                    string renameColumnCommand = $@"
                            EXEC sp_rename '{nombreTabla}.{nombreCampo}', '{columna.nombre}', 'COLUMN'";
                    sql.Database.ExecuteSqlCommand(renameColumnCommand);
                }
             
                if (columna.primaryKey)
                {
                    // Verificar si ya existe una clave primaria en la tabla
                    string checkPrimaryKeyQuery = $@"
                            SELECT COUNT(*)
                            FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
                            WHERE TABLE_NAME = '{nombreTabla}'
                              AND CONSTRAINT_TYPE = 'PRIMARY KEY'";
                    var primaryKeyExists = sql.Database.SqlQuery<int>(checkPrimaryKeyQuery).Single();

                    if (primaryKeyExists > 0)
                    {
                        return Content(HttpStatusCode.BadRequest, new { message = $"La tabla ya tiene una clave primaria. No se puede agregar otra." });
                    }

                    // Crear la clave primaria en la columna
                    string addPrimaryKeyCommand = $@"
                        ALTER TABLE {nombreTabla}
                        ADD CONSTRAINT PK_{nombreTabla}_{columna.nombre} PRIMARY KEY ({columna.nombre})";
                    sql.Database.ExecuteSqlCommand(addPrimaryKeyCommand);
                }

                return Content(HttpStatusCode.OK, new { message = $"La columna '{nombreCampo}' ha sido actualizada exitosamente." });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}