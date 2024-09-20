using System;
using System.Collections.Generic;
using System.Linq;

namespace UGameCore.Utilities
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

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
                    return str.ToLowerInvariant();
            }
            return str;
        }

        public static bool ContainsAny(
            this string str, IReadOnlyList<string> containedStrings, StringComparison stringComparison)
        {
            int n = containedStrings.Count;
            for (int i = 0; i < n; i++)
            {
                if (str.Contains(containedStrings[i], stringComparison))
                    return true;
            }

            return false;
        }

        public static bool ContainsAnyChar(
            this string str, string containedChars, StringComparison stringComparison)
        {
            for (int i = 0; i < containedChars.Length; i++)
            {
                if (str.Contains(containedChars[i], stringComparison))
                    return true;
            }

            return false;
        }

        public static string SubstringCountClamped(this string str, int count)
        {
            if (count < 0)
                throw new ArgumentException();

            return str[..Math.Min(count, str.Length)];
        }
    }
}
