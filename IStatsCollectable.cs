using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Object from which statistics can be collected.
    /// </summary>
    public interface IStatsCollectable
    {
        public class Context
        {
            public Dictionary<string, StringBuilder> StringBuildersPerCategory { get; private set; } = new(StringComparer.OrdinalIgnoreCase);
            public string categoryToProcess;
            public bool allowExpensiveProcessing = true;
            public int initialCapacity = 1024;
            public string currentPrefix { get; private set; } = string.Empty;
            public const string kIndentPrefixString = "\t";


            public Context Clone()
            {
                Context context = (Context)this.MemberwiseClone();
                context.StringBuildersPerCategory = new(this.StringBuildersPerCategory, this.StringBuildersPerCategory.Comparer);
                return context;
            }

            public StringBuilder GetStringBuilderForCategory(string category)
            {
                if (this.categoryToProcess != null && !this.categoryToProcess.Equals(category, StringComparison.OrdinalIgnoreCase))
                    return null;

                if (StringBuildersPerCategory.TryGetValue(category, out var sb))
                    return sb;

                sb = new StringBuilder(this.initialCapacity);
                StringBuildersPerCategory[category] = sb;
                return sb;
            }

            public void IncreaseIndent()
            {
                this.currentPrefix += kIndentPrefixString;
            }

            public void DecreaseIndent()
            {
                if (this.currentPrefix.EndsWith(kIndentPrefixString, StringComparison.OrdinalIgnoreCase))
                    this.currentPrefix = this.currentPrefix[..^kIndentPrefixString.Length];
            }

            public void AppendLine(StringBuilder sb, string str)
            {
                sb.Append(this.currentPrefix);
                sb.AppendLine(str);
            }

            public void AppendStringBuilder(StringBuilder sb, StringBuilder stringBuilderToAppend)
            {
                sb.Append(this.currentPrefix);
                sb.Append(stringBuilderToAppend);
            }

            public void AppendOtherContext(StringBuilder sb, Context other)
            {
                foreach (var pair in other.StringBuildersPerCategory)
                {
                    sb.Append(pair.Value);
                }
            }

            public void AppendCollection<T>(StringBuilder sb, IEnumerable<T> collection, string name, bool addObjectTypeCounts)
            {
                sb.Append(this.currentPrefix);
                sb.Append(name);
                sb.Append(": ");
                bool hasCount = collection.TryGetCountFast(out var count);
                if (hasCount)
                    sb.Append(count);
                else
                    sb.Append('?');
                sb.AppendLine();

                if (addObjectTypeCounts)
                {
                    this.IncreaseIndent();
                    this.AppendCollectionObjectTypesCounts(sb, collection);
                    this.DecreaseIndent();
                }
            }

            public void AppendCollectionObjectTypesCounts<T>(StringBuilder sb, IEnumerable<T> collection)
            {
                var dict = new Dictionary<Type, int>();
                foreach (T item in collection)
                {
                    if (item == null)
                        continue;

                    var type = item.GetType();
                    dict.TryGetValue(type, out int count);
                    count++;
                    dict[type] = count;
                }

                foreach (var pair in dict.OrderByDescending(_ => _.Value))
                {
                    sb.Append(this.currentPrefix);
                    sb.Append(pair.Key.Name);
                    sb.Append(": ");
                    sb.Append(pair.Value);
                    sb.AppendLine();
                }
            }
        }

        void GetCategories(List<string> categories) => GetCategoriesDefault(this, categories);

        static string GetDefaultCategory(IStatsCollectable statsCollectable) => statsCollectable.GetType().Name;

        public static void GetCategoriesDefault(IStatsCollectable statsCollectable, List<string> categories)
        {
            categories.Add(GetDefaultCategory(statsCollectable));
        }

        void DumpStats(Context context) => DumpStatsDefault(this, context);

        public static void DumpStatsDefault(IStatsCollectable statsCollectable, Context context)
        {
            StringBuilder sb = context.GetStringBuilderForCategory(GetDefaultCategory(statsCollectable));
            if (sb == null)
                return;

            Type type = statsCollectable.GetType();

            System.Reflection.FieldInfo[] fields = type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            System.Reflection.PropertyInfo[] properties = type.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            foreach (System.Reflection.FieldInfo field in fields)
                DumpMember(statsCollectable, sb, context, field);
            
            foreach (System.Reflection.PropertyInfo property in properties)
                DumpMember(statsCollectable, sb, context, property);
        }

        static void DumpMember(IStatsCollectable statsCollectable, StringBuilder sb, Context context, System.Reflection.MemberInfo memberInfo)
        {
            var fieldInfo = memberInfo as System.Reflection.FieldInfo;
            var propertyInfo = memberInfo as System.Reflection.PropertyInfo;

            if (propertyInfo != null && !propertyInfo.CanRead)
                return;

            Type memberType = fieldInfo?.FieldType ?? propertyInfo.PropertyType;
            
            if (!ShouldDumpMemberWithType(memberType))
                return;

            string memberName = memberInfo.Name;

            object value = null;
            bool bGetValue = F.RunExceptionSafe(() => value = fieldInfo != null ? fieldInfo.GetValue(statsCollectable) : propertyInfo.GetValue(statsCollectable));

            if (!bGetValue)
            {
                context.AppendLine(sb, $"{memberName}: <exception>");
                return;
            }

            if (null == value)
            {
                context.AppendLine(sb, $"{memberName}: null");
                return;
            }

            if (value is IConvertible convertible)
            {
                context.AppendLine(sb, $"{memberName}: {convertible.ToString(CultureInfo.InvariantCulture)}");
                return;
            }

            if (value is IStatsCollectable nestedStatsCollectable)
            {
                context.AppendLine(sb, $"{memberName}:");
                context.IncreaseIndent();

                Context nestedContext = context.Clone();
                nestedContext.StringBuildersPerCategory.Clear(); // make sure existing StringBuilders are not modified
                nestedContext.categoryToProcess = null; // allow processing all categories
                nestedStatsCollectable.DumpStats(nestedContext);
                context.AppendOtherContext(sb, nestedContext);

                context.DecreaseIndent();
                return;
            }
        }

        static bool ShouldDumpMemberWithType(Type type)
        {
            return typeof(IStatsCollectable).IsAssignableFrom(type)
                || typeof(IConvertible).IsAssignableFrom(type);
        }
    }
}
