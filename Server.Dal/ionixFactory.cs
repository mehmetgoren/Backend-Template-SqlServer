namespace Server.Dal
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.IO;
    using ionix.Data;
    using ionix.Data.SqlServer;
    using ionix.Utils.Extensions;

    public static class ionixFactory
    {
        #region Connection String Dependecy Injection

        private static Type _connectionStringProviderType;

        public static void SetConnectionStringProviderType<TConnectionStringProvider>()
            where TConnectionStringProvider : IConnectionStringProvider
        {
            _connectionStringProviderType = typeof(TConnectionStringProvider);
        }

        private static IConnectionStringProvider _connectionStringProvider;

        public static IConnectionStringProvider ConnectionStringProvider
        {
            get
            {
                if (null == _connectionStringProvider)
                {
                    if (null == _connectionStringProviderType)
                        throw new NullReferenceException(
                            "Please set SetConnectionStringProviderType via dependency injection");

                    _connectionStringProvider =
                        (IConnectionStringProvider) Activator.CreateInstance(_connectionStringProviderType);
                }
                return _connectionStringProvider;
            }
        }

        private static DbConnection CreateDbConnection(DB db)
        {
            DbConnection conn = new SqlConnection();
            conn.ConnectionString = ConnectionStringProvider.GetConnectionString(db);
            conn.Open();

            return conn;
        }

        #endregion

        public static Action<ExecuteSqlCompleteEventArgs> OnLogSqlScript;



        public static IDbAccess CreatDataAccess(DB db)
        {
            var connection = CreateDbConnection(db);
            DbAccess dataAccess = new DbAccess(connection);

            if (null != OnLogSqlScript)
                dataAccess.ExecuteSqlComplete += new ExecuteSqlCompleteEventHandler(OnLogSqlScript);

            return dataAccess;
        }

        public static ITransactionalDbAccess CreateTransactionalDataAccess(DB db)
        {
            var connection = CreateDbConnection(db);
            TransactionalDbAccess dataAccess = new TransactionalDbAccess(connection);

            if (null != OnLogSqlScript)
                dataAccess.ExecuteSqlComplete += new ExecuteSqlCompleteEventHandler(OnLogSqlScript);

            return dataAccess;
        }

        internal static ICommandFactory CreateFactory(IDbAccess dataAccess)
        {
            return new CommandFactory(dataAccess);
        }

        //Orn Custom type ve select işlemleri için.
        internal static ICommandAdapter CreateCommand(IDbAccess dataAccess)
        {
            return new CommandAdapter(CreateFactory(dataAccess), CreateEntityMetaDataProvider);
        }


        public static DbClient CreateDbClient(DB db = DB.Default)
        {
            return new DbClient(CreatDataAccess(db));
        }

        public static TransactionalDbClient CreateTransactionalDbClient(DB db = DB.Default)
        {
            return new TransactionalDbClient(CreateTransactionalDataAccess(db));
        }

        public static DbContext CreateDbContext()
        {
            var dbAccess = CreatDataAccess(DB.Default);
            return new DbContext(dbAccess);
        }

        public static TransactionalDbContext CreateTransactionalDbContext()
        {
            var transactionalDbAccess = CreateTransactionalDataAccess(DB.Default);
            return new TransactionalDbContext(transactionalDbAccess);
        }


        //use non transactional operations only.
        internal static TRepository CreateRepository<TRepository>(IDbAccess dataAccess)
            where TRepository : IDisposable
        {
            var cmd = CreateCommand(dataAccess);
            return (TRepository)Activator.CreateInstance(typeof(TRepository), cmd);
        }
        public static TRepository CreateRepository<TRepository>()
            where TRepository : IDisposable
        {
            return CreateRepository<TRepository>(CreatDataAccess(DB.Default));
        }


        public static IEntityMetaDataProvider CreateEntityMetaDataProvider()
        {
            return DbSchemaMetaDataProvider.Instance;
        }


        public static TEntity CreateEntity<TEntity>()
            where TEntity : new()
        {
            TEntity entity = new TEntity();
            var metaData = CreateEntityMetaDataProvider().CreateEntityMetaData(typeof(TEntity));
            metaData["OpDate"]?.Property.SetValue(entity, DateTime.Now);
            metaData["OpIp"]?.Property.SetValue(entity, "127.0.0.0");
            metaData["OpUserId"]?.Property.SetValue(entity, 1);

            return entity;
        }

        //
        public static IFluentPaging CreatePaging()
        {
            return new FluentPaging();
        }

        public static void BulkCopy<T>(IEnumerable<T> list, ICommandAdapter cmd)
        {
            if (!list.IsEmptyList())
            {
                BulkCopyCommand bulkCopyCommand = new BulkCopyCommand(cmd.Factory.DataAccess.Cast<DbAccess>().Connection.Cast<SqlConnection>());
                bulkCopyCommand.Execute(list, CreateEntityMetaDataProvider());
            }
        }
    }
}
