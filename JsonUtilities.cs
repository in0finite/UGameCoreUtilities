using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;

namespace UGameCore.Utilities
{
    public static class JsonUtilities
    {
        class IgnorePropertiesResolver : DefaultContractResolver
        {
            public (Type type, string name)[] ignoredProperties = Array.Empty<(Type type, string name)>();

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                JsonProperty jsonProperty = base.CreateProperty(member, memberSerialization);
                string memberName = member.Name;

                if (Array.Exists(
                    this.ignoredProperties,
                    _ => _.name.Equals(memberName, StringComparison.Ordinal) && member.DeclaringType.IsAssignableFrom(_.type)))
                {
                    jsonProperty.ShouldSerialize = _ => false;
                }

                return jsonProperty;
            }
        }

        // StringEnumConverter seems like immutable object, so it can be cached
        static readonly JsonConverter[] s_jsonConverters = new[] { new StringEnumConverter() };

        // Don't cache other objects, the functions can be called recursively (by serialized properties).
        // If necessary, the caller can cache his own instance of JsonSerializerSettings and pass it manually,
        // to avoid memory allocations.



        static JsonSerializerSettings CreateSettings()
        {
            return new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Converters = s_jsonConverters,
            };
        }

        public static string ToJson(this object obj, JsonSerializerSettings settings)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, settings);
        }

        public static string ToJson(this object obj)
        {
            var settings = CreateSettings();
            return obj.ToJson(settings);
        }

        public static string ToJsonWithMaxDepth(this object obj, int maxDepth)
        {
            var settings = CreateSettings();
            settings.MaxDepth = maxDepth;
            return obj.ToJson(settings);
        }

        public static string ToJsonIgnoreProperties(this object obj, params string[] properties)
        {
            Type type = obj.GetType();
            
            var contractResolver = new IgnorePropertiesResolver();
            contractResolver.ignoredProperties = Array.ConvertAll(properties, _ => (type, _));

            var settings = CreateSettings();
            settings.ContractResolver = contractResolver;
            return obj.ToJson(settings);
        }
    }
}
