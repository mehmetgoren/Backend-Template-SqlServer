namespace Server.Rest
{
    public interface IServerMonitor
    {
        string GetCpuInfo();

        string GetOsVersion();
        int GetProcessorCount();
        double GetCpuUsage();
        double GetMemoryUsage();
        double GetDiskUsage();
    }
}
