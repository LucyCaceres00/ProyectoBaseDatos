using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;


namespace Proyecto_LucyCaceres.Models
{
    public partial class MySqlDatabaseEntities : DbContext
    {
        public MySqlDatabaseEntities()
        : base("name=MySqlDatabaseEntities") // Usar el nombre de la cadena de conexión en el archivo web.config
        {
        }

        public MySqlDatabaseEntities(string connectionString)
            : base(connectionString) // Constructor adicional para aceptar cadenas de conexión dinámicas
        {
        }

    }
}
