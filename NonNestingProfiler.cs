using System.Collections.Generic;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Wrapper around <see cref="IProfiler"/> which doesn't allow nesting of sections, ie maintains all
    /// sections on 1 level of hierarchy.
    /// </summary>
    public class NonNestingProfiler : IProfiler
    {
        IProfiler m_profiler;
        bool m_sectionOpened = false;


        public NonNestingProfiler(IProfiler profiler)
        {
            m_profiler = profiler;
        }

        public void BeginSection(string name)
        {
            if (m_sectionOpened)
            {
                m_sectionOpened = false;
                m_profiler.EndSection();
            }

            m_profiler.BeginSection(name);
        }

        public void Clear()
        {
            m_sectionOpened = false;
            m_profiler.Clear();
        }

        public void EndSection()
        {
            if (!m_sectionOpened)
                return;
            m_sectionOpened = false;
            m_profiler.EndSection();
        }

        public IEnumerable<IProfiler.Section> GetSections(long parentSectionId)
        {
            return m_profiler.GetSections(parentSectionId);
        }
    }
}
