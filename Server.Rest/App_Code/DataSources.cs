namespace Server.Rest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Reflection;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    public static class DataSources
    {
        public static string Translate(this string str, string lang)
        {
            if (!String.IsNullOrEmpty(str) && !String.IsNullOrEmpty(lang))
            {
                if (DataSources.Jsons.LanguageDictionary.TryGetValue(str, out var item))
                {
                    var pi = item.GetType().GetProperty(lang, BindingFlags.Instance | BindingFlags.Public);
                    if (null != pi)
                        return pi.GetValue(item)?.ToString();
                }
            }

            return str;
        }

        public static class Jsons
        {
            public sealed class LangValue
            {
                public string en { get; set; }
                public string tr { get; set; }
            }
            private static readonly object SyncLanguageDictionary = new object();
            private static IDictionary<string, LangValue> _LanguageDictionary;
            public static IDictionary<string, LangValue> LanguageDictionary
            {
                get
                {
                    _LanguageDictionary = null;
                    if (null == _LanguageDictionary)
                    {
                        lock (SyncLanguageDictionary)
                        {
                            if (null == _LanguageDictionary)
                            {
                                string path = Directory.GetCurrentDirectory();

                                path += $"/assets/jsons/{nameof(LanguageDictionary)}.json";

                                string json = File.ReadAllText(path);

                                var dic = JsonConvert.DeserializeObject<Dictionary<string, LangValue>>(json);
                                var newOne = new Dictionary<string, LangValue>();
                                foreach (var kvp in dic)
                                {
                                    newOne.Add(kvp.Key.Trim(), kvp.Value);
                                }

                                _LanguageDictionary = newOne;
                            }
                        }
                    }

                    return _LanguageDictionary;
                }
            }

            public static class AppSettings
            {
                private static readonly IConfigurationRoot File =
                    new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();

                public static class ConnectionStrings
                {
                    public static string Default => File[$"{nameof(ConnectionStrings)}:{nameof(Default)}"];
                }

                public static class Server
                {
                    public static string Address
                    {
                        get
                        {
                            string value = File[$"{nameof(Server)}:{nameof(Address)}"];
                            if (String.IsNullOrEmpty(value))
                            {
                                IPAddress ipv4 = Dns.GetHostAddresses(String.Empty)
                                     .FirstOrDefault(p => p.AddressFamily == AddressFamily.InterNetwork);

                                if (null != ipv4)
                                    return "http://" + ipv4 + ":1881";
                            }

                            return value;
                        }
                    }

                    public static string SqlPath => File[$"{nameof(Server)}:{nameof(SqlPath)}"];

                    public static string SqlErrorPath => File[$"{nameof(Server)}:{nameof(SqlErrorPath)}"];
                }
            }
        }

        public static class InMemory
        {
        }
    }
}
