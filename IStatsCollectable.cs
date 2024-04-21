using System;
using System.Collections.Generic;
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
            public readonly Dictionary<string, StringBuilder> StringBuildersPerCategory = new(StringComparer.OrdinalIgnoreCase);
            public string categoryToProcess;
            public bool allowExpensiveProcessing = true;
            public int initialCapacity = 1024;
            public string currentPrefix = string.Empty;
            public const string kIndentPrefixString = "\t";

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

        void GetCategories(List<string> categories);

        void DumpStats(Context context);
    }
}
