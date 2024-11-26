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
    }
}