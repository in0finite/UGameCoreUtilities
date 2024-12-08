using System;
using System.Collections.Generic;

namespace UGameCore.Utilities
{
    public static class EnumExtensions
    {
        static class EnumGenericStaticCache<T>
            where T : unmanaged, Enum
        {
            public struct CacheData
            {
                public Dictionary<T, string> NamesPerValue;
                public T[] Values;
                public string[] Names;
            }

            [ThreadStatic] public static CacheData? Cache;


            public static void EnsureCached()
            {
                if (Cache.HasValue)
                    return;

                CacheData c = new CacheData();
                Array arrayValues = Enum.GetValues(typeof(T));
                c.Values = new T[arrayValues.Length];
                c.NamesPerValue = new Dictionary<T, string>(arrayValues.Length);
                for (int i = 0; i < arrayValues.Length; i++)
                {
                    object valueObj = arrayValues.GetValue(i);
                    T genericValue = (T)valueObj;
                    c.Values[i] = genericValue;
                    c.NamesPerValue.TryAdd(genericValue, genericValue.ToString());
                }
                c.Names = Enum.GetNames(typeof(T));

                Cache = c;
            }
        }

        /// <summary>
        /// Get name of this Enum value from cache.
        /// </summary>
        public static string GetCachedEnumName<T>(this T enumValue)
            where T : unmanaged, Enum
        {
            EnumGenericStaticCache<T>.EnsureCached();
            Dictionary<T, string> dict = EnumGenericStaticCache<T>.Cache.Value.NamesPerValue;
            // undefined Enum values will not be cached, so we have to call ToString() for them
            return dict.TryGetValue(enumValue, out string str) ? str : enumValue.ToString();
        }
    }
}
