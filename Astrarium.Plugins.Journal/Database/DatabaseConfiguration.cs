using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Database
{
    public class DatabaseConfiguration : DbConfiguration
    {
        public DatabaseConfiguration()
        {
            var providerServicesType = typeof(System.Data.SQLite.EF6.SQLiteProviderFactory).Assembly.DefinedTypes.First(x => x.Name == "SQLiteProviderServices");
            var providerServices = (DbProviderServices)Activator.CreateInstance(providerServicesType);

            SetProviderServices("System.Data.SQLite", providerServices);
            SetProviderServices("System.Data.SQLite.EF6", providerServices);
            SetProviderFactory("System.Data.SQLite", System.Data.SQLite.SQLiteFactory.Instance);
            SetProviderFactory("System.Data.SQLite.EF6", System.Data.SQLite.EF6.SQLiteProviderFactory.Instance);
            SetDefaultConnectionFactory(new DatabaseConnectionFactory());
        }
    }
}
