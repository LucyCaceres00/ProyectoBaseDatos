using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
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
        //private TransitoEntities db = new TransitoEntities();
        // GET: Tabla
        [HttpGet]
        [Route("api/Tabla/getTablas")]
        [ResponseType(typeof(TablasVM))]
        public IHttpActionResult getTablas(bool isSql)
        {
            List<TablasVM> tablas = new List<TablasVM>();

            if (isSql)
            {
                tablas = sql.Database.SqlQuery<TablasVM>(@"
                                    SELECT 
                                        name AS nombre,
                                        create_date AS fechaCreacion
                                    FROM 
                                        sys.tables").ToList();

            }

            if (tablas.Count == 0)
            {
                return NotFound();
            }

            return Ok(tablas);
        }
    }
}