namespace Server.Rest
{
    using System.Linq;
    using System.Threading.Tasks;
    using ionix.Utils.Extensions;
    using Microsoft.AspNetCore.SignalR;
    using Models;
    using Microsoft.Extensions.DependencyInjection;

    public sealed class ServerMonitoringHub : BaseHub
    {
        private static readonly IHubContext<ServerMonitoringHub> _context = Startup.ServiceProvider.GetService<IHubContext<ServerMonitoringHub>>();

        public void Start()
        {
            ServerMonitoringService.Instance.Start();

            ServerMonitoringService.Instance.Ticked -= this.OnTick;
            ServerMonitoringService.Instance.Ticked += this.OnTick;
        }

        public void Stop()
        {
            ServerMonitoringService.Instance.Ticked -= this.OnTick;

            ServerMonitoringService.Instance.Stop();
        }


        private async void OnTick(object sender, ServerMonitoringEventArgs e)
        {
            await OnServerMonitoringTicked(e);
        }
        private Task OnServerMonitoringTicked(ServerMonitoringEventArgs e)
        {
            ServerInfo info = e.ServerInfo;

            ChartModel cmCpu = CreateMonitoringChartModel("Cpu Usage %", info.CpuUsage, "Available Cpu %", 100.0);
            ChartModel cmRam = CreateMonitoringChartModel("Memory Usage (MB)", info.MemoryUsage, "Available Memory (MB)", 100.0 - info.MemoryUsage);
            ChartModel cmHdd = CreateMonitoringChartModel("HDD Usage (MB)", info.DiskUsage, "Available HDD Alanı (MB)", 100.0 - info.DiskUsage);

           return _context.Clients.All.InvokeAsync("notify", new {info, cmCpu, cmRam, cmHdd});
        }

        private static ChartModel CreateMonitoringChartModel(string usedText, object usedValue, string availableText, object availableValue)
        {
            ChartModel ret = new ChartModel();
            ChartModelDataSet dataSet = new ChartModelDataSet();
            ret.datasets = dataSet.ToSingleItemList().ToList();

            ret.labels.Add(usedText);
            dataSet.data.Add(usedValue);

            ret.labels.Add(availableText);
            dataSet.data.Add(availableValue);

            string color = Utility.CreateRandomColorCode();
            dataSet.backgroundColor.Add(color);
            dataSet.hoverBackgroundColor.Add(color);

            return ret;
        }

        //private bool temp;
        protected override void Dispose(bool disposing)
        {
            //temp = true;

            //if (disposing)
            //{
            //    this._timer?.Dispose();
            //    this._timer = null;
            //}
            base.Dispose(disposing);
        }

        //
    }
}