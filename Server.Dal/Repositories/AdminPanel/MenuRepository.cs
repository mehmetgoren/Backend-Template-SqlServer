namespace Server.Dal
{
    using ionix.Data;
    using Models;

    public class MenuRepository : Repository<Menu>
    {
        public MenuRepository(ICommandAdapter cmd)
            : base(cmd) { }


    }
}
