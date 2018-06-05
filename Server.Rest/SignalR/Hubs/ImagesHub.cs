namespace Server.Rest
{
    using System.IO;
    using System.Threading;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.DependencyInjection;

    public class ImagesHub : BaseHub
    {
        private static readonly IHubContext<ImagesHub> _context = Startup.ServiceProvider.GetService<IHubContext<ImagesHub>>();

        public void FloodImages()
        {
            string path = Directory.GetCurrentDirectory();

            path += "/assets/images";

            foreach (string fileName in Directory.GetFiles(path))
            {
                _context.Clients.All.InvokeAsync("notify", File.ReadAllBytes(fileName));
                Thread.Sleep(16);//60FPS
            }
        }
    }
}
