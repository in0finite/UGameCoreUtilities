using System.Collections.Generic;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Provides profiling and analytics of code execution, such are timings, call count, etc.
    /// </summary>
    public interface IProfiler
    {
        public struct Section
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public ulong NumCalls { get; set; }
            public double TotalDurationMs { get; set; }
        }

        /// <summary>
        /// Start a new section.
        /// </summary>
        void BeginSection(string name);

        /// <summary>
        /// End the current section.
        /// </summary>
        void EndSection();

        /// <summary>
        /// Get id of currently opened section. Returns -1 if there is no active section.
        /// </summary>
        long CurrentSectionId { get; }

        bool TryGetSection(long sectionId, out Section section);

        /// <summary>
        /// Get all sections that are child of given section. Pass in -1 to get root sections.
        /// </summary>
        IEnumerable<Section> GetSections(long parentSectionId);

        /// <summary>
        /// Clear all data held by profiler.
        /// </summary>
        void Clear();
    }
}
