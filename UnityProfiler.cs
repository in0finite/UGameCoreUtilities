using System.Collections.Generic;

namespace UGameCore.Utilities
{
    public class UnityProfiler : IProfiler
    {
        readonly DefaultProfiler m_defaultProfiler = new();


        public long CurrentSectionId => m_defaultProfiler.CurrentSectionId;

        public void BeginSection(string name)
        {
            m_defaultProfiler.BeginSection(name);
            UnityEngine.Profiling.Profiler.BeginSample(name);
        }

        public void Clear()
        {
            while (m_defaultProfiler.CurrentSectionId != -1)
            {
                m_defaultProfiler.EndSection();
                UnityEngine.Profiling.Profiler.EndSample();
            }

            m_defaultProfiler.Clear();
            //UnityEngine.Profiling.Profiler.EndThreadProfiling();
        }

        public void EndSection()
        {
            var previousSectionId = m_defaultProfiler.CurrentSectionId;
            m_defaultProfiler.EndSection();

            if (previousSectionId != m_defaultProfiler.CurrentSectionId)
                UnityEngine.Profiling.Profiler.EndSample();
        }

        public long GetParentSectionId(long sectionId)
        {
            return m_defaultProfiler.GetParentSectionId(sectionId);
        }

        public IEnumerable<IProfiler.Section> GetSections(long parentSectionId)
        {
            return m_defaultProfiler.GetSections(parentSectionId);
        }

        public bool TryGetSection(long sectionId, out IProfiler.Section section)
        {
            return m_defaultProfiler.TryGetSection(sectionId, out section);
        }
    }
}
