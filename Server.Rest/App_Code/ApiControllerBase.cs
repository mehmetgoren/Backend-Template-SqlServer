namespace Server.Rest
{
    using Microsoft.AspNetCore.Mvc;
    using Server.Dal;
    using System;
    using System.Collections.Generic;
    using ionix.Data;

    public abstract class ApiControllerBase : Controller
    {
        public virtual bool IsModelValid<TEntity>(TEntity model)
        {
            var result = model.IsModelValid();// EntityMetadaExtensions.IsModelValid(model);
            return result;
        }

        public virtual bool IsModelValid<TEntity>(IEnumerable<TEntity> modelList)
        {
            return modelList.IsModelListValid();// EntityMetadaExtensions.IsModelListValid(modelList);
        }

        private readonly Lazy<DbContext> _dbContext = new Lazy<DbContext>(ionixFactory.CreateDbContext, true);
        public DbContext Db => this._dbContext.Value;


        public TransactionalDbContext CreateTransactionalDbContext()
        {
            return ionixFactory.CreateTransactionalDbContext();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this._dbContext.IsValueCreated)
                    this._dbContext.Value.Dispose();
            }
            base.Dispose(disposing);
        }

        public override JsonResult Json(object data)
        {
            return new DefaultJsonResult(data);
        }
    }
}
