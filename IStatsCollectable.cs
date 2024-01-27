using System;
using System.Collections.Generic;
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

            public void AppendCollection<T>(StringBuilder sb, IEnumerable<T> collection, string name)
            {
                bool hasCount = collection.TryGetCountFast(out var count);
                sb.Append(name);
                sb.Append(": ");
                if (hasCount)
                    sb.Append(count);
                else
                    sb.Append('?');
                sb.AppendLine();
            }
        }

        void GetCategories(List<string> categories);

        void DumpStats(Context context);
    }
}
