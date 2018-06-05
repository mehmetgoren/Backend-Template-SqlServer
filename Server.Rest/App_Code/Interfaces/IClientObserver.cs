namespace Server.Rest
{
    public interface IClientObserver
    {
        void Notify(string groupName, string opName, string message);
    }
}
