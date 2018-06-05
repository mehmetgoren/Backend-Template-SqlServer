namespace Server.Models
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;


    public sealed class Field
    {
        public string ColumnName { get; set; }
        public bool IsKey { get; set; }
        public bool IsNullable { get; set; }
        public int MaxLength { get; set; }
        public bool ReadOnly { get; set; }
        public string RegularExpression { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public JavascriptType JsType { get; set; }
    }

    public enum JavascriptType
    {
        @string = 0,
        number = 1,
        Date = 2,
        boolean = 3,
        Uint8Array = 4,
        any = 5,
    }
}
