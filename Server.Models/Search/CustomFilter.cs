namespace Server.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ionix.Data;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public sealed class CustomFilter//Extra ve standart json da olmayan alanlar için eklenöiş.
    {
        public string Field { get; set; }//ın Serverside equals to PropertyName
        public ConditionOperator Operator { get; set; }
        public IEnumerable<object> Values { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public JsType DataType { get; set; }

        public FilterCriteria ToCriteria(char prefix)
        {
            if (this.Values != null)
            {
                List<object> convertedValues = new List<object>(this.Values.Count());
                foreach (object value in this.Values)
                {
                    if (value != null && !String.IsNullOrEmpty(value.ToString()))
                    {
                        object convertedValue = null;
                        switch (this.DataType)
                        {
                            case JsType.boolean:
                                convertedValue = Convert.ChangeType(value, TypeCode.Boolean);
                                break;
                            case JsType.date:
                                convertedValue = Convert.ChangeType(value, TypeCode.DateTime);
                                break;
                            case JsType.number:
                                convertedValue = Convert.ChangeType(value, TypeCode.Decimal);
                                break;
                            default:
                                convertedValue = value.ToString();
                                break;
                        }

                        convertedValues.Add(convertedValue);
                    }
                }

                return new FilterCriteria(this.Field, this.Operator, prefix, convertedValues.ToArray());
            }
            return null;
        }
    }
}
