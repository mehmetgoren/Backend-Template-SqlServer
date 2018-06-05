namespace Server.Dal
{
    public enum DB
    {
        Default
    }

    public interface IConnectionStringProvider
    {
        string GetConnectionString(DB db);
    }
}
