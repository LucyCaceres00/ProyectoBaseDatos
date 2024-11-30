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
                                    c2.name AS Campo2
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
                                    t1.name = @nombreTabla OR t2.name = @nombreTabla";

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
                return InternalServerError(ex);
            }
        }

    }
}
