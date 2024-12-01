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
    public class QuerysController : ApiController
    {
        private SqlDatabaseEntities sql = new SqlDatabaseEntities();
        private MySqlDatabaseEntities mySql = new MySqlDatabaseEntities();

        [HttpPost]
        [Route("api/executeQuery")]
        public IHttpActionResult ExecuteQuery([FromBody] QuerysVM request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Query))
                {
                    return Content(HttpStatusCode.BadRequest, new { message = "La consulta SQL no puede estar vacía." });
                }

                using (var connection = sql.Database.Connection)
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = request.Query;
                        if (request.Query.TrimStart().StartsWith("EXEC", StringComparison.OrdinalIgnoreCase) ||
                                        request.Query.TrimStart().StartsWith("EXECUTE", StringComparison.OrdinalIgnoreCase))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;

                            // Ejecutar el procedimiento almacenado y obtener resultados
                            var reader = command.ExecuteReader();
                            var results = new List<Dictionary<string, object>>();

                            while (reader.Read())
                            {
                                var row = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row[reader.GetName(i)] = reader.GetValue(i);
                                }
                                results.Add(row);
                            }
                            return Ok(new { results });
                        }
                        else if (request.Query.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                        {
                            var reader = command.ExecuteReader();
                            var results = new List<Dictionary<string, object>>();

                            while (reader.Read())
                            {
                                var row = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row[reader.GetName(i)] = reader.GetValue(i);
                                }
                                results.Add(row);
                            }
                            return Ok(new { results });
                        }
                        else
                        {
                            // Para consultas como INSERT, DELETE, UPDATE, etc.
                            int affectedRows = command.ExecuteNonQuery();
                            return Ok(new { message = $"Consulta ejecutada correctamente. Filas afectadas: {affectedRows}." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new { error = ex.Message });
            }
        }
    }
}
