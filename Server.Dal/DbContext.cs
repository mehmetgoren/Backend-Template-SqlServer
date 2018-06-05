namespace Server.Dal
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using ionix.Data;
    using ionix.Utils.Extensions;
    using Models;

    //Ayrıca DB.Default u da buradan yönetebilirsin
    public class DbContext : IDisposable
    {
        protected IDbAccess DbAccess { get; }

        public DbContext(IDbAccess dbAccess)
        {
            this.DbAccess = dbAccess ?? throw new ArgumentNullException(nameof(dbAccess));

            this._action = new Lazy<ActionRepository>(() => ionixFactory.CreateRepository<ActionRepository>(this.DbAccess), true);
            this._appSetting = new Lazy<AppSettingRepository>(() => ionixFactory.CreateRepository<AppSettingRepository>(this.DbAccess), true);
            this._appUser = new Lazy<AppUserRepository>(() => ionixFactory.CreateRepository<AppUserRepository>(this.DbAccess), true);
            this._controller = new Lazy<ControllerRepository>(() => ionixFactory.CreateRepository<ControllerRepository>(this.DbAccess), true);
            this._menu = new Lazy<MenuRepository>(() => ionixFactory.CreateRepository<MenuRepository>(this.DbAccess), true);
            this._roleAction = new Lazy<RoleActionRepository>(() => ionixFactory.CreateRepository<RoleActionRepository>(this.DbAccess), true);
            this._roleMenu = new Lazy<RoleMenuRepository>(() => ionixFactory.CreateRepository<RoleMenuRepository>(this.DbAccess), true);
            this._role = new Lazy<RoleRepository>(() => ionixFactory.CreateRepository<RoleRepository>(this.DbAccess), true);
        }

        public int ExecuteNonQuery(string sql, params object[] pars)
        {
            return this.DbAccess.ExecuteNonQuery(sql.ToQuery(pars));
        }

        public int ExecuteNonQuery(string sql)
        {
            return this.DbAccess.ExecuteNonQuery(sql.ToQuery());
        }

        private readonly Lazy<ActionRepository> _action;
        public ActionRepository Action => this._action.Value;

        private readonly Lazy<AppSettingRepository> _appSetting;
        public AppSettingRepository AppSetting => this._appSetting.Value;

        private readonly Lazy<AppUserRepository> _appUser;
        public AppUserRepository AppUser => this._appUser.Value;

        private readonly Lazy<ControllerRepository> _controller;
        public ControllerRepository Controller => this._controller.Value;

        private readonly Lazy<MenuRepository> _menu;
        public MenuRepository Menu => this._menu.Value;

        private readonly Lazy<RoleActionRepository> _roleAction;
        public RoleActionRepository RoleAction => this._roleAction.Value;

        private readonly Lazy<RoleMenuRepository> _roleMenu;
        public RoleMenuRepository RoleMenu => this._roleMenu.Value;

        private readonly Lazy<RoleRepository> _role;
        public RoleRepository Role => this._role.Value;

        public IEnumerable<V_RoleMenu> GetV_RoleMenuList(int roleId)
        {
            return ionixFactory.CreateCommand(this.DbAccess).Query<V_RoleMenu>(V_RoleMenu.Query(roleId));
        }

        public IEnumerable<V_Menu> GetV_MenuList()
        {
            return ionixFactory.CreateCommand(this.DbAccess).Query<V_Menu>(V_Menu.Query());
        }


        public virtual void Dispose()
        {
            this?.DbAccess.Dispose();
        }
    }

    // it' s also like UnitOfWork.
    public class TransactionalDbContext : DbContext, IDbTransaction
    {
        protected new ITransactionalDbAccess DbAccess => base.DbAccess.Cast<ITransactionalDbAccess>();

        public TransactionalDbContext(ITransactionalDbAccess dbAccess) : base(dbAccess)
        {

        }

        public void Commit()
        {
            this.DbAccess.Commit();
        }

        public void Rollback()
        {
            this.DbAccess.Rollback();
        }

        IDbConnection IDbTransaction.Connection => this.DbAccess.Connection;

        public IsolationLevel IsolationLevel => this.DbAccess.IsolationLevel;
    }
}
