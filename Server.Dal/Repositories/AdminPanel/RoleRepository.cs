namespace Server.Dal
{
    using ionix.Data;
    using System.Collections.Generic;
    using Models;


    public class RoleRepository : Repository<Role>
    {
        public RoleRepository(ICommandAdapter cmd)
            : base(cmd) { }


        public IEnumerable<V_RoleControllerAction> Select_V_RoleControllerAction()
        {
            return this.Cmd.Query<V_RoleControllerAction>(V_RoleControllerAction.Query());
        }

        public IList<Role> SelectAdminsOnly()
        {
            return this.Select(" where IsAdmin <> 1".ToQuery());
        }

        public Role SelectByName(string name)
        {
            return this.SelectSingle(" where Name=@0".ToQuery(name));
        }

        public Role SelectById(int id)
        {
            return this.SelectSingle(" where Id=@0".ToQuery(id));
        }


        public V_RoleAppUser SelectVievByDbUserId(int appUserId)
        {

            var q = V_RoleAppUser.Query().ToInnerQuery("t");
            q.Sql(" where t.AppUserId=@0", appUserId);
            return this.Cmd.QuerySingle<V_RoleAppUser>(q);
           // return this.Cmd.SelectSingle(Fluent.Where<V_RoleAppUser>().Equals(u => u.AppUserId.Value, appUserId));
        }
    }
}
