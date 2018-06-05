namespace Server.Rest
{
    using System;
    using System.Diagnostics;
    using Models;

    public sealed class ServerMonitoringEventArgs
    {
        public ServerInfo ServerInfo { get; set; }
    }

    public sealed class ServerMonitoringService: NanoServiceBase, IDisposable
    {
        public static readonly ServerMonitoringService Instance = new ServerMonitoringService(TimeSpan.FromSeconds(2), new WindowsServerMonitor());

        private readonly IServerMonitor _serverMonitor;
        private ServerMonitoringService(TimeSpan interval, IServerMonitor serverMonitor)
            : base(interval)
        {
            this._serverMonitor = serverMonitor;
        }


        private ServerInfo _info;
        public event EventHandler<ServerMonitoringEventArgs> Ticked;
        protected override void OnTick()
        {
            var evnt = this.Ticked;
            if (null != evnt)
            {
                try
                {
                    if (null == this._info)
                    {
                        ServerInfo info = new ServerInfo()
                        {
                            OsVersion = this._serverMonitor.GetOsVersion(),
                            ProcessorCount = this._serverMonitor.GetProcessorCount(),
                            CpuUsage = this._serverMonitor.GetCpuUsage(),
                            MemoryUsage = this._serverMonitor.GetMemoryUsage(),
                            DiskUsage = this._serverMonitor.GetDiskUsage(),
                            ActiveUserCount = TokenTable.Instance.ActiveUserCount
                        };

                        this._info = info;
                    }
                    else
                    {
                        this._info.CpuUsage = this._serverMonitor.GetCpuUsage();
                        this._info.ActiveUserCount = TokenTable.Instance.ActiveUserCount;
                    }
                    evnt.Invoke(this, new ServerMonitoringEventArgs() { ServerInfo = this._info });
                }
                catch (Exception ex)
                {
                    SQLog.Logger.Create(new StackTrace()).Code(8541).OnException(ex).SaveAsync();
                }
            }
        }

        public void Dispose()
        {
            this.Stop();
        }
    }
}
