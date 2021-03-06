﻿namespace Server.Dal
{
    using System;
    using System.Data;
    using ionix.Data;

    public interface IDbClient : IDisposable
    {
        IDbAccess DataAccess { get; }
        ICommandAdapter Cmd { get; }
    }

    public abstract class DbClient<TDbAccess> : IDbClient
        where TDbAccess : IDbAccess
    {
        IDbAccess IDbClient.DataAccess => this.DataAccess;
        public TDbAccess DataAccess { get; }

        protected DbClient(TDbAccess dataAccess)
        {
            this.DataAccess = dataAccess;
        }

      //  public ICommandFactory Factory => ionixFactory.CreateFactory(this.DataAccess);

        public ICommandAdapter Cmd => ionixFactory.CreateCommand(this.DataAccess);

        public virtual void Dispose()
        {
            if (null != this.DataAccess)
                this.DataAccess.Dispose();
        }
    }

    public sealed class DbClient : DbClient<IDbAccess>
    {
        internal DbClient(IDbAccess dbAccess)
            : base(dbAccess)
        {
        }
    }

    public sealed class TransactionalDbClient : DbClient<ITransactionalDbAccess>, IDbTransaction
    {
        internal TransactionalDbClient(ITransactionalDbAccess transactionalDbAccess)
            : base(transactionalDbAccess)
        {
        }

        public void Commit()
        {
            this.DataAccess.Commit(); ;
        }

        public IsolationLevel IsolationLevel => this.DataAccess.IsolationLevel;

        public void Rollback()
        {
            this.DataAccess.Rollback();
        }

        IDbConnection IDbTransaction.Connection => this.DataAccess.Connection;
    }
}
