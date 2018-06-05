namespace Server.Models
{
    public sealed class ServerInfo
    {
        public string OsVersion { get; set; }

        public int ProcessorCount { get; set; }
        public double CpuUsage { get; set; }

        public double MemoryUsage { get; set; }

        public double DiskUsage { get; set; }

        public int ActiveUserCount { get; set; }
    }
}
