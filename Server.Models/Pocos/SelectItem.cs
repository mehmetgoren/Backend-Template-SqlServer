namespace Server.Models
{
    public sealed class SelectItem
    {
        //[JsonProperty("value")]
        public object value { get; set; }

        // [JsonProperty("label")]
        public string label { get; set; }
    }
}
