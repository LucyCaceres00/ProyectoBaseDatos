using System;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using Proyecto_LucyCaceres.Models;

namespace Proyecto_LucyCaceres.Controllers
{
    public class RelacionController : ApiController
    {
        private SqlDatabaseEntities sql = new SqlDatabaseEntities();
        private MySqlDatabaseEntities mySql = new MySqlDatabaseEntities();

        [HttpGet]
        [Route("api/getRelacionesSql/{nombreTabla}")]
        [ResponseType(typeof(RelacionesVM))]
        public IHttpActionResult ObtenerRelaciones(string nombreTabla)
        {
            try
            {
                string query = @"
                                SELECT 
                                    fk.name AS NombreRelacion,
                                    t1.name AS Tabla1,
                                    c1.name AS Campo1,
                                    t2.name AS Tabla2,
                                    c2.name AS Campo2,
                                    'Llave foranea' AS TipoRelacion
                                FROM 
                                    sys.foreign_keys fk
                                INNER JOIN 
                                    sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
                                INNER JOIN 
                                    sys.tables t1 ON fkc.parent_object_id = t1.object_id
                                INNER JOIN 
                                    sys.columns c1 ON fkc.parent_object_id = c1.object_id AND fkc.parent_column_id = c1.column_id
                                INNER JOIN 
                                    sys.tables t2 ON fkc.referenced_object_id = t2.object_id
                                INNER JOIN 
                                    sys.columns c2 ON fkc.referenced_object_id = c2.object_id AND fkc.referenced_column_id = c2.column_id
                                WHERE 
                                    t1.name = @nombreTabla OR t2.name = @nombreTabla

                                UNION ALL

                                    SELECT 
                                        kc.name AS NombreRelacion,
                                        t.name AS Tabla1,
                                        c.name AS Campo1,
                                        '' AS Tabla2,
                                        '' AS Campo2,
                                        'Llave primaria' AS TipoRelacion
                                    FROM 
                                        sys.key_constraints kc
                                    INNER JOIN 
                                        sys.index_columns ic ON kc.parent_object_id = ic.object_id AND kc.unique_index_id = ic.index_id
                                    INNER JOIN 
                                        sys.tables t ON kc.parent_object_id = t.object_id
                                    INNER JOIN 
                                        sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                                    WHERE 
                                        kc.type = 'PK' AND t.name = @nombreTabla";

                var relaciones = sql.Database.SqlQuery<RelacionesVM>(query,
                    new SqlParameter("@nombreTabla", nombreTabla)).ToList();

                if (relaciones.Count == 0)
                {
                    return Content(HttpStatusCode.BadRequest, new { message = $"No se encontraron relaciones para la tabla '{nombreTabla}'." });
                }

                return Ok(relaciones);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }

        [HttpPut]
        [Route("api/eliminarRelacion/{nombreTabla}/{llave}")]
        public IHttpActionResult EliminarLlavesForaneas(string nombreTabla, string llave)
        {
            try
            {
                // Obtener las llaves foráneas relacionadas con la tabla
                string obtenerLlavesQuery = @"
                        SELECT fk.name AS NombreRelacion
                        FROM sys.foreign_keys fk
                        INNER JOIN sys.tables t ON fk.parent_object_id = t.object_id
                        WHERE t.name = @nombreTabla";

                var llavesForaneas = sql.Database.SqlQuery<string>(obtenerLlavesQuery,
                    new SqlParameter("@nombreTabla", nombreTabla)).ToList();

                if (llavesForaneas.Count == 0)
                {
                    return Content(HttpStatusCode.BadRequest, new { message = $"No se encontraron llaves foráneas para la tabla '{nombreTabla}'." });
                }

                string eliminarLlaveQuery = $@"
                            ALTER TABLE {nombreTabla}
                            DROP CONSTRAINT {llave}";
                sql.Database.ExecuteSqlCommand(eliminarLlaveQuery);

                return Content(HttpStatusCode.OK, new { message = $"Se elimino la llave {llave} foránea asociadas a la tabla '{nombreTabla}'." });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/agregarRelacion")]
        public IHttpActionResult CrearRelacion([FromBody] RelacionesVM relacion)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(relacion.Tabla1) || string.IsNullOrWhiteSpace(relacion.Campo1) ||
                    string.IsNullOrWhiteSpace(relacion.Tabla2) || string.IsNullOrWhiteSpace(relacion.Campo2))
                {
                    return Content(HttpStatusCode.BadRequest, new { message = "Los nombres de las tablas y los campos son obligatorios." });
                }

                if (!string.IsNullOrWhiteSpace(relacion.TablaIntermedia))
                {
                    CrearRelacionMuchosAMuchos(relacion);
                }
                else
                {
                    CrearRelacionUnoAMuchos(relacion);
                }

                return Content(HttpStatusCode.OK, new { message = "La relación fue creada con exitoso." });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }

        private void CrearRelacionUnoAMuchos(RelacionesVM relacion)
        {
            string query = $@"
                            ALTER TABLE {relacion.Tabla2}
                            ADD CONSTRAINT FK_{relacion.Tabla2}_{relacion.Tabla1}
                            FOREIGN KEY ({relacion.Campo2})
                            REFERENCES {relacion.Tabla1}({relacion.Campo1});";

            sql.Database.ExecuteSqlCommand(query);
        }

        private void CrearRelacionMuchosAMuchos(RelacionesVM relacion)
        {
            // Crear la tabla intermedia si no existe
                string crearTablaIntermediaQuery = $@"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{relacion.TablaIntermedia}')
                BEGIN
                    CREATE TABLE {relacion.TablaIntermedia} (
                        {relacion.Campo1} INT NOT NULL,
                        {relacion.Campo2} INT NOT NULL
                    );
                END;";

                sql.Database.ExecuteSqlCommand(crearTablaIntermediaQuery);

                // Agregar columnas a la tabla intermedia si no existen
                string agregarColumnasIntermediaQuery = $@"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                               WHERE TABLE_NAME = '{relacion.TablaIntermedia}' AND COLUMN_NAME = '{relacion.Campo1}')
                BEGIN
                    ALTER TABLE {relacion.TablaIntermedia}
                    ADD {relacion.Campo1} INT NOT NULL;
                END;

                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                               WHERE TABLE_NAME = '{relacion.TablaIntermedia}' AND COLUMN_NAME = '{relacion.Campo2}')
                BEGIN
                    ALTER TABLE {relacion.TablaIntermedia}
                    ADD {relacion.Campo2} INT NOT NULL;
                END;";

                sql.Database.ExecuteSqlCommand(agregarColumnasIntermediaQuery);

                // Crear una clave primaria compuesta en la tabla intermedia
                string crearClavePrimariaCompuestaQuery = $@"
                    IF NOT EXISTS (SELECT * 
                                   FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
                                   WHERE TABLE_NAME = '{relacion.TablaIntermedia}' AND CONSTRAINT_TYPE = 'PRIMARY KEY')
                    BEGIN
                        ALTER TABLE {relacion.TablaIntermedia}
                        ADD CONSTRAINT PK_{relacion.TablaIntermedia}
                        PRIMARY KEY ({relacion.Campo1}, {relacion.Campo2});
                    END;";

                sql.Database.ExecuteSqlCommand(crearClavePrimariaCompuestaQuery);

                // Crear llaves foráneas en la tabla intermedia
                string crearLlavesForaneasQuery = $@"
                    IF NOT EXISTS (SELECT * 
                                   FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
                                   WHERE CONSTRAINT_NAME = 'FK_{relacion.TablaIntermedia}_{relacion.Tabla1}')
                    BEGIN
                        ALTER TABLE {relacion.TablaIntermedia}
                        ADD CONSTRAINT FK_{relacion.TablaIntermedia}_{relacion.Tabla1}
                        FOREIGN KEY ({relacion.Campo1})
                        REFERENCES {relacion.Tabla1}({relacion.Campo1});
                    END;

                    IF NOT EXISTS (SELECT * 
                                   FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
                                   WHERE CONSTRAINT_NAME = 'FK_{relacion.TablaIntermedia}_{relacion.Tabla2}')
                    BEGIN
                        ALTER TABLE {relacion.TablaIntermedia}
                        ADD CONSTRAINT FK_{relacion.TablaIntermedia}_{relacion.Tabla2}
                        FOREIGN KEY ({relacion.Campo2})
                        REFERENCES {relacion.Tabla2}({relacion.Campo2});
                    END;";

                    sql.Database.ExecuteSqlCommand(crearLlavesForaneasQuery);
               
        }

    }
}
