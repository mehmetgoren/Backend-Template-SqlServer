namespace Server.Dal
{
    using ionix.Data;
    using Models;

    public class AppSettingRepository : Repository<AppSetting>
    {
        public AppSettingRepository(ICommandAdapter cmd)
            : base(cmd)
        {
            
        }

        public int DeleteAll()
        {
            return this.DataAccess.ExecuteNonQuery("delete from AppSetting".ToQuery());
        }

    }
}
