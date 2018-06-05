namespace Server.Rest
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Reflection;
    using Dal;
    using ionix.Data;
    using ionix.Utils;
    using ionix.Utils.Extensions;
    using Models;
    using Newtonsoft.Json;


    public struct SearchResult
    {
        public IEnumerable<object> EntityList { get; set; }
        public int Total { get; set; }
    }

    public class SearchCriteriaResolver//Customize ederken bu nesneyi türet.
    {
        private static object TrimIfStringValue(object propertyValue, PropertyInfo pi)
        {
            if (null == propertyValue || pi.PropertyType != CachedTypes.String)
                return propertyValue;

            return ((String)propertyValue).Trim();
        }


        public virtual void OnSetFilterCriteria(SqlQuery where, PropertyInfo pi, object entityValue)
        {
            if (pi.PropertyType == CachedTypes.String)
            {
                where.Sql(pi.Name)
                    .Sql(" LIKE @")
                    .Sql(pi.Name);

                where.Parameter(pi.Name, "%" + entityValue + "%");
            }
            else
            {
                where.Sql(pi.Name)
                    .Sql("= @")
                    .Sql(pi.Name);

                where.Parameter(pi.Name, entityValue);
            }
        }

        public SearchResult Search(SearchParams searchParams)
        {
            var request = searchParams.Request;
            var take = searchParams.Take;
            var page = searchParams.Page;
            var sort = searchParams.Sort;

            SearchResult ret = new SearchResult();

            Type entityType = Metadata.GetType(request.TypeFullName);
            if (null == entityType)
                throw new NotFoundException(request.TypeFullName);

            object entity = JsonConvert.DeserializeObject(request.EntityJson, entityType);

            IEntityMetaData metaData = new EntityMetaDataProvider().CreateEntityMetaData(entityType);

            ISqlQueryProvider builder = entity as ISqlQueryProvider; 

            if (null == builder)
            {
                builder = new SqlQueryBuilderSelect(metaData);
            }

            SqlQuery query = builder.ToQuery().ToInnerQuery();

            SqlQuery where = new SqlQuery(" WHERE ");
            int count = 0;

            HashSet<string> betweenColumns = new HashSet<string>();
            IEnumerable<BetweenAttribute> bas = entityType.GetCustomAttributes<BetweenAttribute>();
            if (!bas.IsEmptyList())
            {
                foreach (BetweenAttribute ba in bas)
                {
                    betweenColumns.Add(ba.FirstProperty);
                    betweenColumns.Add(ba.SecondProperty);

                    betweenColumns.Add(ba.ColumnName); //?

                    object firstVal = entityType.GetProperty(ba.FirstProperty).GetValue(entity);
                    if (!firstVal.IsNull())
                    {
                        object secVal = entityType.GetProperty(ba.SecondProperty).GetValue(entity);
                        if (secVal.IsNull())
                        {
                            where.Sql("T.")
                                .Sql(ba.ColumnName)
                                .Sql(" >= @")
                                .Parameter(ba.FirstProperty, firstVal);
                        }
                        else
                        {
                            where.Sql("T.")
                                .Sql(ba.ColumnName)
                                .Sql(" BETWEEN @")
                                .Sql(ba.FirstProperty)
                                .Sql(" AND @")
                                .Sql(ba.SecondProperty)
                                .Sql(" AND ")
                                .Parameter(ba.FirstProperty, firstVal)
                                .Parameter(ba.SecondProperty, secVal);
                        }

                        ++count;
                    }
                }
            }

            foreach (PropertyMetaData pmd in metaData.Properties)
            {
                if (betweenColumns.Contains(pmd.Schema.ColumnName)) continue;

                PropertyInfo pi = pmd.Property;
                if (pi.GetCustomAttribute<NotMappedAttribute>() != null)
                    continue;

                object entityValue = pi.GetValue(entity);
                entityValue = TrimIfStringValue(entityValue, pi);
                if (null != entityValue && entityValue.ToString().Length != 0)
                {
                    object defaultValue = Utility.GetSearchDefaultValue(pi.PropertyType);
                    if (!Object.Equals(defaultValue, entityValue)) //Kullanıcı Değer Girdi İse.
                    {
                        where.Sql("T.");
                       // Type propertyType = pi.PropertyType;

                        FilterOperatorAttribute foa = pi.GetCustomAttribute<FilterOperatorAttribute>();
                        if (null != foa)
                        {
                            where.Sql(pi.Name);
                            bool flag = true;
                            switch (foa.Operator)
                            {
                                case ConditionOperator.Equals:
                                    where.Sql(" = @");
                                    break;
                                case ConditionOperator.Contains:
                                    where.Sql(" LIKE @");
                                    where.Parameter(pi.Name, "%" + entityValue + "%");
                                    flag = false;
                                    break;
                                case ConditionOperator.StartsWith:
                                    where.Sql(" LIKE @");
                                    where.Parameter(pi.Name, entityValue + "%");
                                    flag = false;
                                    break;
                                case ConditionOperator.EndsWith:
                                    where.Sql(" LIKE @");
                                    where.Parameter(pi.Name, "%" + entityValue);
                                    flag = false;
                                    break;
                                case ConditionOperator.GreaterThan:
                                    where.Sql(" > @");
                                    break;
                                case ConditionOperator.GreaterThanOrEqualsTo:
                                    where.Sql(" >= @");
                                    break;
                                case ConditionOperator.LessThan:
                                    where.Sql(" < @");
                                    break;
                                case ConditionOperator.LessThanOrEqualsTo:
                                    where.Sql(" <= @");
                                    break;
                                case ConditionOperator.NotEquals:
                                    where.Sql(" <> @");
                                    break;
                                default:
                                    throw new NotSupportedException(foa.Operator.ToString());
                            }
                            where.Sql(pi.Name);
                            if (flag)
                                where.Parameter(pi.Name, entityValue);
                        }
                        else
                        {
                            this.OnSetFilterCriteria(where, pi, entityValue);
                            //if (propertyType == CachedTypes.String)
                            //{
                            //    where.Sql(pi.Name)
                            //        .Sql(" LIKE @")
                            //        .Sql(pi.Name);

                            //    where.Parameter(pi.Name, "%" + entityValue + "%");
                            //}
                            //else
                            //{
                            //    where.Sql(pi.Name)
                            //        .Sql("= @")
                            //        .Sql(pi.Name);

                            //    where.Parameter(pi.Name, entityValue);
                            //}
                        }

                        FilterExtendedSqlAttribute fesa = pi.GetCustomAttribute<FilterExtendedSqlAttribute>();
                        if (null != fesa && !String.IsNullOrEmpty(fesa.Sql))
                        {
                            where.Sql(fesa.Sql);
                        }

                        where.Text.Append(" AND ");

                        ++count;
                    }
                }
            }

            if (count > 0)
            {
                where.Text.Remove(where.Text.Length - 5, 4);
                query.Combine(where);
            }

            SqlQuery countQueryTemp = query;
            bool pagingEnabled = take.HasValue && page.HasValue;
            if (pagingEnabled) //Yani Sayfalama var ise ???....
            {
                if (sort.IsEmptyList())
                {
                    PropertyMetaData pm = metaData.Properties.FirstOrDefault((p) => p.Schema.IsKey);
                    if (null == pm) //Hiç pk yok ise.
                    {
                        pm = metaData.Properties.First();
                    }

                    sort = new[] { new SearchSortRequest() { field = pm.Schema.ColumnName, dir = "asc" } };
                }

                query = query.ToPagingQuery(sort.First(), page.Value, take.Value);
            }

            using (var c = ionixFactory.CreateDbClient())//Bu da Parametrik Olacak.
            {
                ret.EntityList = (IEnumerable<object>)c.Cmd.QueryNonGeneric(entityType, query);

                if (pagingEnabled)
                {
                    SqlQuery countQuery = "SELECT COUNT(*) FROM ( ".ToQuery();
                    countQuery.Combine(countQueryTemp);
                    countQuery.Sql(" ) T");

                    ret.Total = c.Cmd.QuerySingle<int>(countQuery);
                }
            }

            return ret;
        }
    }
}
