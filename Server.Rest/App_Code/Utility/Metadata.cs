namespace Server.Rest
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Reflection;
    using Dal;
    using ionix.Utils;
    using ionix.Utils.Extensions;
    using ionix.Utils.Reflection;
    using Models;

    public static class Metadata
    {
        private static readonly string ModelsAssemblyName = Assembly.GetExecutingAssembly().FullName.Split('.')[0] + ".Models";// "Server.Models";
        private static readonly Assembly ModelAssembly = GetAssembly(ModelsAssemblyName);

        private static Assembly GetAssembly(string asmName)
        {
            return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName.StartsWith(asmName)) ??
               Assembly.Load(asmName);
        }

        private static readonly ConcurrentDictionary<string, Type> dic = new ConcurrentDictionary<string, Type>();

        internal static Type GetType(string typeFullName)
        {
            if (!string.IsNullOrEmpty(typeFullName))
            {
                Type type;
                if (!dic.TryGetValue(typeFullName, out type))
                {
                    type = ModelAssembly.GetType(typeFullName);
                    dic.TryAdd(typeFullName, type);
                }
                return type;
            }
            return null;
        }

        public static Dictionary<string, IEnumerable<Field>> Get(HashSet<string> typeFullNameList)
        {
            Dictionary<string, IEnumerable<Field>> ret = new Dictionary<string, IEnumerable<Field>>();
            if (!typeFullNameList.IsEmptyList())
            {
                foreach (var typeFullName in typeFullNameList)
                {
                    if (!String.IsNullOrEmpty(typeFullName))
                    {
                        Type t = GetType(typeFullName);
                        if (null != t)
                        {
                            List<Field> fields = new List<Field>();
                            var metaData = ionixFactory.CreateEntityMetaDataProvider().CreateEntityMetaData(t);
                            metaData.Properties.ForEach(i =>
                            {
                                Field f = new Field();
                                f.CopyPropertiesFrom(i.Schema);

                                var rea = i.Property.GetCustomAttribute<RegularExpressionAttribute>();
                                if (null != rea && !String.IsNullOrEmpty(rea.Pattern))
                                {
                                    f.RegularExpression = rea.Pattern;
                                }

                                f.JsType = i.Property.PropertyType.ToJsType();

                                fields.Add(f);
                            });

                            ret.Add(typeFullName, fields);
                        }
                    }
                }
            }

            return ret;
        }
    }



    internal static class JavaScriptExtensions
    {
        private static readonly object TypeMapSync = new object();
        private static Dictionary<Type, JavascriptType> typeMap;
        private static Dictionary<Type, JavascriptType> TypeMap
        {
            get
            {
                if (null == typeMap)
                {
                    lock (TypeMapSync)
                    {
                        if (null == typeMap)
                        {
                            typeMap = new Dictionary<Type, JavascriptType>();

                            typeMap.Add(CachedTypes.Byte, JavascriptType.number);
                            typeMap.Add(CachedTypes.SByte, JavascriptType.number);
                            typeMap.Add(CachedTypes.Int16, JavascriptType.number);
                            typeMap.Add(CachedTypes.UInt16, JavascriptType.number);
                            typeMap.Add(CachedTypes.Int32, JavascriptType.number);
                            typeMap.Add(CachedTypes.UInt32, JavascriptType.number);
                            typeMap.Add(CachedTypes.Int64, JavascriptType.number);
                            typeMap.Add(CachedTypes.UInt64, JavascriptType.number);
                            typeMap.Add(CachedTypes.Single, JavascriptType.number);
                            typeMap.Add(CachedTypes.Double, JavascriptType.number);
                            typeMap.Add(CachedTypes.Decimal, JavascriptType.number);
                            typeMap.Add(CachedTypes.Boolean, JavascriptType.boolean);
                            typeMap.Add(CachedTypes.String, JavascriptType.@string);
                            typeMap.Add(CachedTypes.Char, JavascriptType.@string);
                            typeMap.Add(CachedTypes.Guid, JavascriptType.@string);
                            typeMap.Add(CachedTypes.DateTime, JavascriptType.Date);
                            typeMap.Add(typeof(DateTimeOffset), JavascriptType.Date);
                            // typeMap.Add(CachedTypes.ByteArray, JavascriptType.Uint8Array); //Byte Array json da base64 string olması lazım.
                            typeMap.Add(CachedTypes.Nullable_Byte, JavascriptType.number);
                            typeMap.Add(CachedTypes.Nullable_SByte, JavascriptType.number);
                            typeMap.Add(CachedTypes.Nullable_Int16, JavascriptType.number);
                            typeMap.Add(CachedTypes.Nullable_UInt16, JavascriptType.number);
                            typeMap.Add(CachedTypes.Nullable_Int32, JavascriptType.number);
                            typeMap.Add(CachedTypes.Nullable_UInt32, JavascriptType.number);
                            typeMap.Add(CachedTypes.Nullable_Int64, JavascriptType.number);
                            typeMap.Add(CachedTypes.Nullable_UInt64, JavascriptType.number);
                            typeMap.Add(CachedTypes.Nullable_Single, JavascriptType.number);
                            typeMap.Add(CachedTypes.Nullable_Double, JavascriptType.number);
                            typeMap.Add(CachedTypes.Nullable_Decimal, JavascriptType.number);
                            typeMap.Add(CachedTypes.Nullable_Boolean, JavascriptType.boolean);
                            typeMap.Add(CachedTypes.Nullable_Char, JavascriptType.@string);
                            typeMap.Add(CachedTypes.Nullable_Guid, JavascriptType.@string);
                            typeMap.Add(CachedTypes.Nullable_DateTime, JavascriptType.Date);
                            typeMap.Add(typeof(DateTimeOffset?), JavascriptType.Date);
                        }
                    }
                }

                return typeMap;
            }
        }

        internal static JavascriptType ToJsType(this Type type)
        {
            // if (pi.Name == "JsonData")
            //   return JavascriptType.any;
            JavascriptType ret;
            if (TypeMap.TryGetValue(type, out ret))
            {
                return ret;
            }
            return JavascriptType.any;
        }

    }
}
