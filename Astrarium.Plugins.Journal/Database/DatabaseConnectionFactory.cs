﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Database
{
    public class DatabaseConnectionFactory : IDbConnectionFactory
    {
        public DbConnection CreateConnection()
        {
            string databasePath = JournalPlugin.DatabaseFilePath;
            string databaseFolder = Path.GetDirectoryName(databasePath);
            if (!Directory.Exists(databaseFolder))
            {
                Directory.CreateDirectory(databaseFolder);
            }
            string connectionString = $"Data Source={databasePath}; datetimeformat=UnixEpoch;datetimekind=Utc;";
            return new SQLiteConnection(connectionString);
        }

        DbConnection IDbConnectionFactory.CreateConnection(string nameOrConnectionString)
        {
            return CreateConnection();
        }
    }
}
