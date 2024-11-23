using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proyecto_LucyCaceres.Controllers
{
    public class AdministradorController : Controller
    {
        // GET: Administrador
        public ActionResult Ciudadano()
        {
            return View();
        }
      
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Rol()
        {
            return View();
        }

        public ActionResult Usuario()
        {
            return View();
        } 
        
        public ActionResult MediosTrasporte()
        {
            return View();
        }
        public ActionResult TipoDocumentos()
        {
            return View();
        }
        public ActionResult TipoDocumentosCiudadanos()
        {
            return View();
        } 
        
        public ActionResult MedioTransporteCiudadano()
        {
            return View();
        }
        public ActionResult Tablas()
        {
            return View();
        }
    }
}