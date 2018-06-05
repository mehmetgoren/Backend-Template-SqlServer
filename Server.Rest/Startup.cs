namespace Server.Rest
{
    using System;
    using System.Reflection;
    using ionix.Rest;
    using ionix.WebSockets;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Serialization;
    using Server.Dal;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            ionixFactory.SetConnectionStringProviderType<ConnectionStringProvider>();
            IndexedRoles.IgnoreCase = true;
            TokenTableParams.SessionTimeout = Config.WebApiSessionTimeout;

            StartNanoServices();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(opt =>
            {
                
            });

            services.AddSingleton<IHubContext<ServerMonitoringHub>, HubContext<ServerMonitoringHub>>();
            services.AddSingleton<IHubContext<ImagesHub>, HubContext<ImagesHub>>();

            //services.AddSingleton<IAdminPanel, AdminPanelImpl>();
            //services.AddSingleton<IUtils, UtilsImpl>();
            //services.AddSingleton<ICatalog, CatalogImpl>();

            //services.AddSignalRCore();

            services.AddSignalR(options =>
            {
                //alpha 2 den sonra değiştir.
                options.JsonSerializerSettings.ContractResolver = new DefaultContractResolver();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //bu fetch e eklenecek.
            if (Config.WebApiAuthEnabled)
                app.UseTokenTableAuthentication(TokenTable.Instance, AuthorizationValidator.Instance);

            app.UseCors(builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "api/{controller}/{action}/{id?}");
            });


            //signalr
            app.UseSignalR(builder =>
            {
                builder.MapHubs(Assembly.GetExecutingAssembly());
                //builder.MapHub<ServerMonitoringHub>("servermonitoring");
                //builder.MapHub<ImagesHub>("images");
            });


            //

            //app.Use(async (context, next) =>
            //{
            //    await context.Response.WriteAsync("Hi From Aps.Net Core");
            //});

            ServiceProvider = serviceProvider;
        }

        private static void StartNanoServices()
        {
           // ServerMonitoringService.Instance.Start();
        }


        //signalr 
        public static IServiceProvider ServiceProvider { get; private set; }


    }
}
