using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proyecto_LucyCaceres.Models
{
    public class RelacionesVM
    {
        public string NombreRelacion { get; set; }
        public string TablaIntermedia { get; set; }
        public string Tabla1 { get; set; }
        public string Campo1 { get; set; }
        public string Tabla2 { get; set; }
        public string Campo2 { get; set; }
    }
}