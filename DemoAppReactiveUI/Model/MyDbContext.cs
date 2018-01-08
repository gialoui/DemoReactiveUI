using log4net;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace DemoAppReactiveUI.Model
{
    public class MyDbContext : DbContext
    {
        // log
        public static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static string DEFAULT_DB_NAME = "epos_retail_v1";

        public MyDbContext() : base(GetConnectionString())
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }

        [DbConfigurationType(typeof(NpgsqlConfiguration))]
        public class NpgsqlConfiguration : System.Data.Entity.DbConfiguration
        {
            public NpgsqlConfiguration()
            {
                System.Data.Entity.DbConfiguration.Loaded += (_, a) =>
                {
                    a.AddDependencyResolver(new DbProviderDependencyResolver(), true);
                };
                SetProviderServices("Npgsql", Npgsql.NpgsqlServices.Instance);
                SetProviderFactory("Npgsql", Npgsql.NpgsqlFactory.Instance);
                SetDefaultConnectionFactory(new Npgsql.NpgsqlConnectionFactory());
            }
        }

        public class DbProviderDependencyResolver : System.Data.Entity.Infrastructure.DependencyResolution.IDbDependencyResolver
        {
            public object GetService(Type type, object key)
            {
                if (type == typeof(System.Data.Common.DbProviderFactory))
                {
                    return Npgsql.NpgsqlFactory.Instance;
                }
                else if (type == typeof(System.Data.Entity.Infrastructure.IProviderInvariantName) && key?.ToString() == "Npgsql.NpgsqlFactory")
                {
                    return new InvariantName();
                }

                return null;
            }

            public IEnumerable<object> GetServices(Type type, object key)
            {
                return new object[] { GetService(type, key) }.ToList().Where(o => o != null);
            }

            private class InvariantName : System.Data.Entity.Infrastructure.IProviderInvariantName
            {
                public string Name { get; } = "Npgsql";
            }
        }

        public static string GetConnectionString()
        {
            var databaseName = Properties.Settings.Default.databaseName;
            if (databaseName.Length <= 0)
            {
                logger.Error("GetConnectionString failed with Settings.Default.databaseName EMPTY. Use default value.");
                // we use epos_retail_v1 as default
                databaseName = DEFAULT_DB_NAME;
            }

            return GetConnectionString(databaseName: databaseName);
        }

        private static string GetConnectionString(string databaseName)
        {
            var databaseHost = Properties.Settings.Default.databaseHost;
            if (databaseHost.Length <= 0)
            {
                logger.Error("GetConnectionString failed with Settings.Default.databaseHost EMPTY. Use default value.");
                // save localhost as default databaseHost
                Properties.Settings.Default.databaseHost = "localhost";
                Properties.Settings.Default.Save();

                // we use localhost as default
                databaseHost = "localhost";
            }
            var connString = "Host=" + databaseHost + ";Port=5432;user id=" + Properties.Settings.Default.databaseUser + ";password=" + Properties.Settings.Default.databasePass + ";database=" + databaseName + ";CommandTimeout=300;Pooling=true;MinPoolSize=0;MaxPoolSize=30;Keepalive=120;";

            return connString;
        }
    }
}