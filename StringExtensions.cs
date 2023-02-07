using System;

namespace UGameCore.Utilities
{
    public static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static string FirstCharToUpper(this string str)
        {
            if (string.Empty == str)
                return str;

            return str[0].ToString().ToUpperInvariant() + str.Substring(1);
        }

        public static string ToLowerIfNotLower(this string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (!char.IsLower(str[i]))
                    return str.ToLower();
            }
            return str;
        }

        public static bool Contains(
            this string str, string containedString, StringComparison stringComparison)
        {
            return str.IndexOf(containedString, stringComparison) >= 0;
        }
    }
}
