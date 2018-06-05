namespace Server.Models
{
    using System;
    using System.Collections.Generic;
    using ionix.Data;

    public sealed class SearchRequest
    {
        public string TypeFullName { get; set; }
        public string EntityJson { get; set; }
        public IEnumerable<CustomFilter> CustomFilters { get; set; }
    }

    public sealed class SearchSortRequest
    {
       // [JsonProperty("field")]
        public string field { get; set; }

        //[JsonProperty("dir")]
        public string dir { get; set; }
    }

    public enum JsType
    { 
        @string,
        number,
        date,
        boolean,
        Object,
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class BetweenAttribute : Attribute
    {
        public BetweenAttribute(string columnName, string firstProperty, string secondProperty)
        {
            this.ColumnName = columnName;
            this.FirstProperty = firstProperty;
            this.SecondProperty = secondProperty;
        }

        public string ColumnName { get; set; }
        public string FirstProperty { get; set; }
        public string SecondProperty { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FilterOperatorAttribute : Attribute
    {
        public FilterOperatorAttribute(ConditionOperator op)
        {
            this.Operator = op;
        }

        public ConditionOperator Operator { get; private set; }
    }

    //Örneğin Grup tablosunda null ve birimid olnalar için eklendi.
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FilterExtendedSqlAttribute : Attribute
    {
        public FilterExtendedSqlAttribute(string sql)
        {
            this.Sql = sql;
        }
        public string Sql { get; private set; }
    }
}
