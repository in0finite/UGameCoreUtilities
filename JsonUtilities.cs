using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace UGameCore.Utilities
{
    public static class JsonUtilities
    {
        [ThreadStatic]
        static JsonSerializerSettings s_serializerSettings = new()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Converters = new[] { new StringEnumConverter() },
        };


        public static string ToJson(this object obj)
        {
            s_serializerSettings.MaxDepth = null;
            return JsonConvert.SerializeObject(obj, Formatting.Indented, s_serializerSettings);
        }

        public static string ToJsonWithMaxDepth(this object obj, int maxDepth)
        {
            s_serializerSettings.MaxDepth = maxDepth;
            return JsonConvert.SerializeObject(obj, Formatting.Indented, s_serializerSettings);
        }
    }
}
