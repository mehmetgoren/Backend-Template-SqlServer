namespace Server.Rest
{
    using Microsoft.Extensions.Configuration;
    using Server.Dal;
    using System.IO;

    public sealed class ConnectionStringProvider : IConnectionStringProvider
    {
        public string GetConnectionString(DB db)
        {
            return DataSources.Jsons.AppSettings.ConnectionStrings.Default;
        }
    }
}
