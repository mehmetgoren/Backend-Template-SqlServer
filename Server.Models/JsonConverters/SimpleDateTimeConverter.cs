namespace Server.Models
{
    using System;
    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class SimpleDateTimeConverter : DateTimeConverterBase
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            object value = reader.Value; //warning boxing detected....
            if (null != value)
            {
                DateTime ret;
                if (DateTime.TryParseExact(value.ToString(), "dd/MM/yyyy HH:mm", null, DateTimeStyles.None, out ret))
                    return ret;
            }
            return null;
        }

        private static readonly string FormatDateTime = "dd'/'MM'/'yyyy HH:mm";
        private static readonly string FormatDate = "dd'/'MM'/'yyyy";

        //Null da çağırmıyor
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DateTime d = (DateTime)value;
            string s = (d.Hour == 0 && d.Minute == 0 && d.Second == 0)
                ? d.ToString(FormatDate)
                : d.ToString(FormatDateTime);
            writer.WriteValue(s);
        }
    }

    //with seconds
    public class SimpleDateTimeConverter2 : DateTimeConverterBase
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            object value = reader.Value; //warning boxing detected....
            if (null != value)
            {
                DateTime ret;
                if (DateTime.TryParseExact(value.ToString(), "dd/MM/yyyy HH:mm:ss", null, DateTimeStyles.None, out ret))
                    return ret;
            }
            return null;
        }

        private static readonly string FormatDateTime = "dd'/'MM'/'yyyy HH:mm:ss";
        private static readonly string FormatDate = "dd'/'MM'/'yyyy";

        //Null da çağırmıyor
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DateTime d = (DateTime)value;
            string s = (d.Hour == 0 && d.Minute == 0 && d.Second == 0)
                ? d.ToString(FormatDate)
                : d.ToString(FormatDateTime);
            writer.WriteValue(s);
        }
    }

    public class IsoDateTimeConverterTR : IsoDateTimeConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var baseRet = base.ReadJson(reader, objectType, existingValue, serializer);
            if (baseRet is DateTime)
            {
                var temp = (DateTime)baseRet;
                temp = temp.AddHours(3);

                return temp;
            }
            return baseRet;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DateTime d = (DateTime)value;
            d = d.AddHours(3);
            base.WriteJson(writer, value, serializer);
        }
    }
}
