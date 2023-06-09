using System.Linq;
using System.Text;
using UnityEngine;

namespace UGameCore.Utilities
{
    public static class ProfilerExtensions
    {
        public static void EndAllSections(this IProfiler profiler)
        {
            while (profiler.CurrentSectionId >= 0)
                profiler.EndSection();
        }

        public static double GetTotalTime(this IProfiler profiler)
        {
            return profiler.GetSections(-1).Sum(_ => _.TotalDurationMs);
        }

        public static void Log(this IProfiler profiler, string prefix = null)
        {
            var stringBuilder = new StringBuilder($"{prefix} total time: {(profiler.GetTotalTime() / 1000):F3} s:\n");
            profiler.Dump(stringBuilder);
            Debug.Log(stringBuilder.ToString());
        }

        public static void Dump(this IProfiler profiler, StringBuilder sb)
        {
            double totalTime = profiler.GetTotalTime();
            foreach (var section in profiler.GetSections(-1).OrderByDescending(_ => _.TotalDurationMs))
                profiler.Dump(section, sb, 0, totalTime);
        }

        public static void Dump(this IProfiler profiler, IProfiler.Section section, StringBuilder sb, int level, double totalTime)
        {
            double childrenTotalDuration = profiler.GetSections(section.Id).Sum(_ => _.TotalDurationMs);
            double selfDuration = section.TotalDurationMs - childrenTotalDuration;

            for (int i = 0; i < level; i++)
                sb.Append("    ");
            
            sb.Append($"{section.Name.PadRight(20).Substring(0, 20)}  |  ");
            sb.Append($"{(section.TotalDurationMs / totalTime * 100).ZeroIfNan():F1}%  |  ");
            sb.Append($"self {(selfDuration / totalTime * 100).ZeroIfNan():F1}%  |  ");
            sb.Append($"calls {section.NumCalls}  |  ");
            sb.Append($"{section.TotalDurationMs:F3} ms  |  ");
            sb.Append($"self {selfDuration:F3} ms");
            sb.AppendLine();

            foreach (var childSection in profiler.GetSections(section.Id).OrderByDescending(_ => _.TotalDurationMs))
                profiler.Dump(childSection, sb, level + 1, totalTime);
        }
    }
}
