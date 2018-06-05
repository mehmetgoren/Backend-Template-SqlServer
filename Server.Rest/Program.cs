namespace Server.Rest
{
    using ionix.Data;
    using ionix.Data.SqlServer;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Server.Dal;
    using System;
    using System.IO;

    public static class Program
    {
        public static void Main(string[] args)
        {
            ionixFactory.OnLogSqlScript = LogSqlScript;
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                //.UseKestrel(options =>
                //{
                //    var ipAddress = IPAddress.Parse("127.0.0.1");
                //    options.Listen(ipAddress, 5000);
                //})
                .UseUrls(DataSources.Jsons.AppSettings.Server.Address)
                .UseStartup<Startup>()
                .Build();


        private static void LogSqlScript(ExecuteSqlCompleteEventArgs e)
        {
            try
            {
                string path = e.Succeeded ? DataSources.Jsons.AppSettings.Server.SqlPath : DataSources.Jsons.AppSettings.Server.SqlErrorPath;

                if (!String.IsNullOrEmpty(path))
                {
                    using (Stream fs = new FileStream(path, FileMode.Append, FileAccess.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            if (e.Query != null)
                            {
                                sw.WriteLine(e.Query.ToParameterlessQuery());
                                sw.WriteLine();
                            }
                        }
                    }
                }
            }
            catch { }
        }
    }
}
