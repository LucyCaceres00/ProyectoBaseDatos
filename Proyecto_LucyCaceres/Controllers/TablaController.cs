using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
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
                return Content(HttpStatusCode.BadRequest, new { message = ex.Message });
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
                return Content(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }

        [HttpPut]
        [Route("api/Tabla/createTableMySql/{tableId}/{nombreTabla}")]
        public IHttpActionResult updateMySQL(int tableId, string nombreTabla)
        {
            if (string.IsNullOrWhiteSpace(nombreTabla))
            {
                return Content(HttpStatusCode.BadRequest, new { message = "El nombre de la tabla es requerido." });
            }

            try
            {
                // Verificar si la tabla con el tableId existe
                var currentTableName = sql.Database.SqlQuery<string>($@"
                            SELECT table_name 
                            FROM information_schema.tables 
                            WHERE table_schema = DATABASE() AND table_id = @p0", tableId).SingleOrDefault();

                if (currentTableName == null)
                {
                    return Content(HttpStatusCode.BadRequest, new { message = $"No se encontró ninguna tabla con el Id '{tableId}'." });
                }

                // Verificar si la nueva tabla ya existe
                var newTableExists = sql.Database.SqlQuery<int>($@"
                            SELECT COUNT(*) 
                            FROM information_schema.tables 
                            WHERE table_schema = DATABASE() AND table_name = @p0", nombreTabla).SingleOrDefault() > 0;

                if (newTableExists)
                {
                    return Content(HttpStatusCode.BadRequest, new { message = $"La tabla '{nombreTabla}' ya existe." });
                }

                // Renombrar la tabla
                sql.Database.ExecuteSqlCommand($@"
                            ALTER TABLE {currentTableName} RENAME TO {nombreTabla}");

                return Content(HttpStatusCode.OK, new { message = $"La tabla '{currentTableName}' ha sido renombrada a '{nombreTabla}' exitosamente." });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/Tabla/createTableMySql/{nombreTabla}")]
        public IHttpActionResult createTablaMySql(string nombreTabla)
        {
            // Validar que el nombre de la tabla no esté vacío y que solo contenga caracteres alfanuméricos y guiones bajos
            if (string.IsNullOrWhiteSpace(nombreTabla) || !Regex.IsMatch(nombreTabla, @"^[a-zA-Z0-9_]+$"))
            {
                return Content(HttpStatusCode.BadRequest, new { message = "El nombre de la tabla no es válido. Solo se permiten letras, números y guiones bajos." });
            }

            try
            {
                // Obtener el nombre de la base de datos desde la conexión activa
                var databaseName = sql.Database.Connection.Database;

                // Verificar si la tabla ya existe en MySQL
                bool tablaExisteSql = sql.Database.SqlQuery<int>($@"
                        SELECT COUNT(*) 
                        FROM information_schema.tables 
                        WHERE table_name = @p0 AND table_schema = @p1", nombreTabla, databaseName).SingleOrDefault() > 0;

                if (tablaExisteSql)
                {
                    return Content(HttpStatusCode.BadRequest, new { message = $"La tabla '{nombreTabla}' ya existe en MySQL." });
                }

                // Crear la tabla en MySQL
                sql.Database.ExecuteSqlCommand($@"
                        CREATE TABLE {nombreTabla} (
                            Id INT AUTO_INCREMENT PRIMARY KEY
                        )");

                return Content(HttpStatusCode.OK, new { message = $"Tabla '{nombreTabla}' fue creada exitosamente en MySQL." });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, new { message = ex.Message });
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
                return Content(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }

        [HttpPut]
        [Route("api/Tabla/vaciarTablaMySQL/{nombreTabla}")]
        public IHttpActionResult vaciarTablaMySQL(string nombreTabla)
        {
            if (string.IsNullOrWhiteSpace(nombreTabla))
            {
                return Content(HttpStatusCode.BadRequest, new { message = "El nombre de la tabla es requerido." });
            }

            try
            {
                // Verificar si hay claves foráneas que referencian la tabla
                var foreignKeysReferencing = sql.Database.SqlQuery<string>($@"
                SELECT kcu.constraint_name
                FROM information_schema.key_column_usage kcu
                JOIN information_schema.table_constraints tc
                ON kcu.constraint_name = tc.constraint_name
                WHERE kcu.referenced_table_name = @p0 AND tc.constraint_type = 'FOREIGN KEY'", nombreTabla).ToList();

                if (foreignKeysReferencing.Count > 0)
                {
                    foreach (var fkName in foreignKeysReferencing)
                    {
                        // Buscar la tabla que tiene la clave foránea que referencia la tabla actual
                        var referencingTable = sql.Database.SqlQuery<string>($@"
                    SELECT TABLE_NAME
                    FROM information_schema.key_column_usage
                    WHERE constraint_name = @p0", fkName).FirstOrDefault();

                        if (referencingTable != null)
                        {
                            // Comprobar si hay registros en la tabla que hace referencia
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

                // Comprobar si la tabla tiene un campo AUTO_INCREMENT
                var haveAutoIncrement = sql.Database.SqlQuery<int>($@"
                SELECT COUNT(COLUMN_NAME)
                FROM information_schema.columns
                WHERE table_name = @p0 AND extra LIKE '%auto_increment%'", nombreTabla).FirstOrDefault();

                if (haveAutoIncrement > 0)
                {
                    // Reiniciar el valor de AUTO_INCREMENT
                    sql.Database.ExecuteSqlCommand($@"
                    ALTER TABLE {nombreTabla} AUTO_INCREMENT = 1");
                }

                return Content(HttpStatusCode.OK, new { message = $"La tabla '{nombreTabla}' ha sido vaciada exitosamente." });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, new { message = ex.Message });
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
                return Content(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }

        [HttpPut]
        [Route("api/Tabla/eliminarTableMySql/{nombreTabla}")]
        public IHttpActionResult eliminarMySQL(string nombreTabla)
        {
            if (string.IsNullOrWhiteSpace(nombreTabla))
            {
                return Content(HttpStatusCode.BadRequest, new { message = "El nombre de la tabla es requerido." });
            }

            try
            {
                // Verificar si hay claves foráneas que referencian la tabla
                var foreignKeysReferencing = sql.Database.SqlQuery<string>($@"
                SELECT kcu.constraint_name
                FROM information_schema.key_column_usage kcu
                JOIN information_schema.table_constraints tc
                ON kcu.constraint_name = tc.constraint_name
                WHERE kcu.referenced_table_name = @p0 AND tc.constraint_type = 'FOREIGN KEY'", nombreTabla).ToList();

                if (foreignKeysReferencing.Count > 0)
                {
                    return Content(HttpStatusCode.BadRequest, new { message = $"La tabla '{nombreTabla}' no se puede eliminar, ya que otras tablas tienen dependencia a esta." });
                }

                // Eliminar la tabla
                sql.Database.ExecuteSqlCommand($@"
                DROP TABLE {nombreTabla}");

                return Content(HttpStatusCode.OK, new { message = $"La tabla '{nombreTabla}' ha sido eliminada exitosamente." });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }

    }
}