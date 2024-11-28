using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;


namespace Proyecto_LucyCaceres.Models
{
    public partial class MySqlDatabaseEntities : DbContext
    {
        public MySqlDatabaseEntities() : base("name=MySqlDatabaseEntities")
        {
        }

        //public virtual DbSet<MyEntity> MyEntities { get; set; }
    }
}