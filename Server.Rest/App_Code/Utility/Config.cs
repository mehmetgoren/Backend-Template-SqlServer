namespace Server.Rest
{
    using Dal;
    using ionix.Data;
    using ionix.Utils.Collections;
    using ionix.Utils.Extensions;
    using Models;

    public static class Config
    {
        private static IndexedEntityList<AppSetting> _ixList;

        private static readonly object SyncIxList = new object();
        private static IndexedEntityList<AppSetting> IxList
        {
            get
            {
                if (null == _ixList)
                {
                    lock (SyncIxList)
                    {
                        if (null == _ixList)
                        {
                            _ixList = IndexedEntityList<AppSetting>.Create(p => p.Name);
                            using (var c = ionixFactory.CreateDbClient())
                            {
                                _ixList.AddRange(c.Cmd.Select<AppSetting>());
                            }
                        }
                    }
                }

                return _ixList;
            }
        }

        private static T GetValue<T>(string name)
        {
            var setting = IxList.Find(name);
            if (null != setting)
            {
                if (setting.Enabled)
                    return setting.Value.ConvertTo<T>();
            }
            //if not exist in db or not enabled
            return default(T);
        }


        private static string _FastReportsPath;
        public static string FastReportsPath => _FastReportsPath ?? (_FastReportsPath = GetValue<string>(nameof(FastReportsPath)));

        private static bool? _WebApiAuthEnabled;
        public static bool WebApiAuthEnabled //GetValue<bool>(nameof(WebApiAuthEnabled));
        {
            get
            {
                if (null == _WebApiAuthEnabled)
                {
                    _WebApiAuthEnabled = GetValue<bool>(nameof(WebApiAuthEnabled));
                }
                return _WebApiAuthEnabled.Value;
            }
        }

        private static double _WebApiSessionTimeout;
        public static double WebApiSessionTimeout
        {
            get
            {
                if (_WebApiSessionTimeout == 0.0)
                {
                    try
                    {
                        _WebApiSessionTimeout = GetValue<double>(nameof(WebApiSessionTimeout));
                        if (_WebApiSessionTimeout <= 0.0)
                            _WebApiSessionTimeout = 10.0;
                    }
                    catch
                    {
                        _WebApiSessionTimeout = 10.0;
                    }
                }

                return _WebApiSessionTimeout;
            }
        }
    }
}
