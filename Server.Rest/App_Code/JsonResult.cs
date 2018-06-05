namespace Server.Rest
{
    using Newtonsoft.Json;

    //Büyük küçük harf farkı için çıkarıldı.
    public class DefaultJsonResult : Microsoft.AspNetCore.Mvc.JsonResult
    {
        internal static readonly JsonSerializerSettings DefaultJsonSerializerSettings = new JsonSerializerSettings();

        public DefaultJsonResult(object value)
            : base(value, DefaultJsonSerializerSettings)
        {
            
        }
    }
}
