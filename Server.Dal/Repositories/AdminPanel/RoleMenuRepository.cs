namespace Server.Dal
{
    using ionix.Data;
    using Models;

    public class RoleMenuRepository : Repository<RoleMenu>
    {
        public RoleMenuRepository(ICommandAdapter cmd)
            : base(cmd) { }

        public int DeleteByRoleId(int roleId)
        {
            var q = "delete from RoleMenu where RoleId=@0".ToQuery(roleId);

            return this.DataAccess.ExecuteNonQuery(q);
        }
    }
}
