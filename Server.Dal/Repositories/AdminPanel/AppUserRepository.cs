namespace Server.Dal
{
    using System.Collections.Generic;
    using ionix.Data;
    using Models;

    public class AppUserRepository : Repository<AppUser>
    {
        public AppUserRepository(ICommandAdapter cmd)
            : base(cmd)
        {
        }

        public IEnumerable<V_AppUser> QueryView()
        {
            return this.Cmd.Query<V_AppUser>(V_AppUser.Query());
        }

        public V_AppUser QueryViewBy(string userName)
        {
            var q = V_AppUser.Query().ToInnerQuery("t").Sql(" where t.Username=@0", userName);
            return this.Cmd.QuerySingle<V_AppUser>(q);
        }
    }
}
