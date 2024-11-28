using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using Proyecto_LucyCaceres.Models;

namespace Proyecto_LucyCaceres.Controllers
{
    public class TablaController : ApiController
    {
        private SqlDatabaseEntities sql = new SqlDatabaseEntities();
        private MySqlDatabaseEntities mySql = new MySqlDatabaseEntities();

        //private TransitoEntities db = new TransitoEntities();
        // GET: Tabla
        [HttpGet]
        [Route("api/Tabla/getTablasSql")]
        [ResponseType(typeof(TablasVM))]
        public IHttpActionResult getTablasSQL()
        {
            List<TablasVM> tablas = new List<TablasVM>();
            tablas = sql.Database.SqlQuery<TablasVM>(@"
                        SELECT 
                             object_id AS Id,
                            name AS nombre,
                            create_date AS fechaCreacion
                        FROM 
                            sys.tables").ToList(); ;

            if (tablas.Count == 0)
            {
                return NotFound();
            }

            return Ok(tablas);
        }

        [HttpGet]
        [Route("api/Tabla/getTablasMySql")]
        [ResponseType(typeof(TablasVM))]
        public IHttpActionResult getTablasMySQL()
        {
            List<TablasVM> tablas = new List<TablasVM>();
            tablas = mySql.Database.SqlQuery<TablasVM>(@"
                        SELECT 
                            TABLE_NAME AS Nombre,
                            CREATE_TIME AS FechaCreacion
                        FROM 
                            information_schema.TABLES
                        WHERE 
                            TABLE_SCHEMA = 'transito'").ToList();

            if (tablas.Count == 0)
            {
                return NotFound();
            }

            return Ok(tablas);
        }

        [HttpPost]
        [Route("api/Tabla/createTableSql/{nombreTabla}")]
        public IHttpActionResult createTablaSql(string nombreTabla)
        {
            if (nombreTabla == "")
            {
                return Content(HttpStatusCode.BadRequest, new { message = "El nombre de la tabla es requerido." });
            }

            try
            {
                // Verificar si la tabla ya existe
                bool tablaExisteSql = sql.Database.SqlQuery<int>($@"
                                SELECT COUNT(*) 
                                FROM sys.tables 
                                WHERE name = @p0", nombreTabla).SingleOrDefault() > 0;

                if (tablaExisteSql)
                {
                    return Content(HttpStatusCode.BadRequest, new { message = $"La tabla '{nombreTabla}' ya existe en SQL Server." });
                }

                // Crear la tabla en SQL Server
                sql.Database.ExecuteSqlCommand($@"
                                CREATE TABLE {nombreTabla} (
                                    Id INT IDENTITY(1,1)
                                )");
                return Content(HttpStatusCode.OK, new { message = $"Tabla '{nombreTabla}' fue creada exitosamente en SQL Server." });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPut]
        [Route("api/Tabla/createTableSql/{tableId}/{nombreTabla}")]
        public IHttpActionResult updateSQL(int tableId, string nombreTabla)
        {
            if (nombreTabla == "")
            {
                return Content(HttpStatusCode.BadRequest, new { message = "El nombre de la tabla es requerido." });
            }

            try
            {
                string currentTableName = sql.Database.SqlQuery<string>($@"
                                    SELECT name 
                                    FROM sys.tables 
                                    WHERE object_id = @p0", tableId).SingleOrDefault();

                if (currentTableName == null)
                {
                    return Content(HttpStatusCode.BadRequest, new { message = $"No se encontró ninguna tabla con el Id '{tableId}'." });
                }

                // Verificar si la nueva tabla ya existe
                bool newTableExists = sql.Database.SqlQuery<int>($@"
                                    SELECT COUNT(*) 
                                    FROM sys.tables 
                                    WHERE name = @p0", nombreTabla).SingleOrDefault() > 0;

                if (newTableExists)
                {
                    return Content(HttpStatusCode.BadRequest, new { message = $"La tabla '{nombreTabla}' ya existe." });
                }

                // Renombrar la tabla
                sql.Database.ExecuteSqlCommand($@"
                                    EXEC sp_rename @currentName, @newName",
                                            new SqlParameter("@currentName", currentTableName),
                                            new SqlParameter("@newName", nombreTabla));

                return Content(HttpStatusCode.OK, new { message = $"La tabla '{currentTableName}' ha sido renombrada a '{nombreTabla}' exitosamente." });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPut]
        [Route("api/Tabla/vaciarTablaSQL/{nombreTabla}")]
        public IHttpActionResult vaciarTablaSQL(string nombreTabla)
        {
            if (nombreTabla == "")
            {
                return Content(HttpStatusCode.BadRequest, new { message = "El nombre de la tabla es requerido." });
            }

            try
            {
                var foreignKeysReferencing = sql.Database.SqlQuery<string>($@"
                        SELECT fk.name 
                        FROM sys.foreign_keys fk
                        INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
                        WHERE fkc.referenced_object_id = OBJECT_ID(@p0)", nombreTabla).ToList();

                if (foreignKeysReferencing.Count > 0)
                {
                    foreach (var fkName in foreignKeysReferencing)
                    {
                        var referencingTable = sql.Database.SqlQuery<string>($@"
                            SELECT OBJECT_NAME(parent_object_id)
                            FROM sys.foreign_keys
                            WHERE name = @p0", fkName).FirstOrDefault();

                        if (referencingTable != null)
                        {
                            var count = sql.Database.SqlQuery<int>($@"
                                SELECT COUNT(*) 
                                FROM {referencingTable}").FirstOrDefault();

                            if (count > 0)
                            {
                                return Content(HttpStatusCode.BadRequest, new { message = $"No se pueden eliminar los datos de la tabla '{nombreTabla}', ya que otras tablas tienen dependencia a esta." });
                            }
                        }
                    }
                }
                
                // Vaciar la tabla
                sql.Database.ExecuteSqlCommand($@"
                DELETE FROM {nombreTabla}");

                var haveIdentity = sql.Database.SqlQuery<int>($@"
                        SELECT COUNT(c.name) 
                        FROM sys.columns c
                        JOIN sys.tables t ON c.object_id = t.object_id
                        WHERE c.is_identity = 1 AND t.name = @p0", nombreTabla).FirstOrDefault();

                if (haveIdentity > 0)
                {
                    // Reiniciar el valor de IDENTITY
                    sql.Database.ExecuteSqlCommand($@"
                    DBCC CHECKIDENT(@p0, RESEED, 0)", nombreTabla);
                }

                return Content(HttpStatusCode.OK, new { message = $"La tabla '{nombreTabla}' ha sido vaciada exitosamente." });
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpPut]
        [Route("api/Tabla/eliminarTableSql/{nombreTabla}")]
        public IHttpActionResult eliminarSQL(string nombreTabla)
        {
            if (nombreTabla == "")
            {
                return Content(HttpStatusCode.BadRequest, new { message = "El nombre de la tabla es requerido." });
            }

            try
            {
                var foreignKeysReferencing = sql.Database.SqlQuery<string>($@"
                        SELECT fk.name 
                        FROM sys.foreign_keys fk
                        INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
                        WHERE fkc.referenced_object_id = OBJECT_ID(@p0)", nombreTabla).ToList();

                if (foreignKeysReferencing.Count > 0)
                {
                    return Content(HttpStatusCode.BadRequest, new { message = $"La tabla '{nombreTabla}' no se puede eliminar, ya que otras tablas tienen dependencia a esta." });

                }

                sql.Database.ExecuteSqlCommand($@"
                        DROP TABLE {nombreTabla}");
                return Content(HttpStatusCode.OK, new { message = $"La tabla '{nombreTabla}' ha sido eliminada exitosamente." });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}