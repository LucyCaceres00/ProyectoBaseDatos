using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proyecto_LucyCaceres.Models
{
    public class CamposVM
    {
        public string tabla { get; set; }
        public string nombre { get; set; }
        public string tipoDato { get; set; }
        public bool isNull { get; set; }
        public int isPrimaryKey { get; set; }
    }
}