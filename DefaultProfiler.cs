using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UGameCore.Utilities
{
    public class DefaultProfiler : IProfiler
    {
        // note: we never remove sections from list, only when explicitly asked to clear everything.
        // so, we can build optimized storage that only grows, but never removes anything from it.

        System.Diagnostics.Stopwatch m_stopwatch = System.Diagnostics.Stopwatch.StartNew();

        List<CustomSection> m_allSections = new List<CustomSection>(64)
        {
            // add fake root section - so we can have only 1 tree, and don't need to implement additional logic
            // for multiple trees
            CreateNewSection(),
        };
        int m_currentParentIndex = 0;

        [StructLayout(LayoutKind.Sequential, Pack = 1)] // save some space, don't expand into 64bit fields
        struct CustomSection
        {
            public IProfiler.Section data;
            public double timeOfLastEntry;

            public int parentIndex;

            public int nextSiblingIndex; // basically a linked list

            public int firstChildIndex;
            public int lastChildIndex;
        }



        static CustomSection CreateNewSection()
        {
            var section = new CustomSection();
            section.data.Id = -1;
            section.parentIndex = -1;
            section.nextSiblingIndex = -1;
            section.firstChildIndex = -1;
            section.lastChildIndex = -1;
            return section;
        }

        int GetChildByName(int parentIndex, string name, out int outPreviousChild)
        {
            outPreviousChild = -1;
            CustomSection parent = m_allSections[parentIndex];

            int child = parent.firstChildIndex;
            while (child != -1)
            {
                if (m_allSections[child].data.Name.Equals(name, System.StringComparison.Ordinal))
                    return child;

                outPreviousChild = child;
                child = m_allSections[child].nextSiblingIndex;
            }

            outPreviousChild = -1;
            return -1;
        }

        public void BeginSection(string name)
        {
            // note: parent always exists, because we have fake root section

            int existingChildIndex = GetChildByName(m_currentParentIndex, name, out int _);

            bool sectionAlreadyExisted = existingChildIndex != -1;

            CustomSection section = !sectionAlreadyExisted
                ? CreateNewSection()
                : m_allSections[existingChildIndex];

            if (!sectionAlreadyExisted)
            {
                // init
                section.data.Id = m_allSections.Count;
                section.data.Name = name;
                section.parentIndex = m_currentParentIndex;
                // assign sibling for previous child
                int previousChildIndex = m_allSections[m_currentParentIndex].lastChildIndex;
                if (previousChildIndex != -1)
                {
                    CustomSection previousChild = m_allSections[previousChildIndex];
                    previousChild.nextSiblingIndex = (int)section.data.Id;
                    m_allSections[previousChildIndex] = previousChild;
                }
            }

            int newSectionIndex = (int)section.data.Id;

            section.timeOfLastEntry = m_stopwatch.Elapsed.TotalMilliseconds;
            section.data.NumCalls++;

            // assign first/last child for parent
            if (!sectionAlreadyExisted)
            {
                CustomSection parent = m_allSections[m_currentParentIndex];
                parent.lastChildIndex = newSectionIndex;
                if (parent.firstChildIndex == -1)
                    parent.firstChildIndex = newSectionIndex;
                m_allSections[m_currentParentIndex] = parent;
            }

            // add or update
            if (!sectionAlreadyExisted)
                m_allSections.Add(section);
            else
                m_allSections[newSectionIndex] = section;

            // change the current parent
            m_currentParentIndex = newSectionIndex;
        }

        public void Clear()
        {
            m_allSections.Clear();
            m_allSections.Add(CreateNewSection()); // fake root section
            m_currentParentIndex = 0;
            m_allSections.TrimExcess();
        }

        public void EndSection()
        {
            if (m_currentParentIndex == 0)
            {
                // no section to close
                return;
            }

            CustomSection currentSection = m_allSections[m_currentParentIndex];

            double timeNow = m_stopwatch.Elapsed.TotalMilliseconds;
            currentSection.data.TotalDurationMs += timeNow - currentSection.timeOfLastEntry;

            m_allSections[m_currentParentIndex] = currentSection;

            m_currentParentIndex = currentSection.parentIndex;
        }

        public long CurrentSectionId => m_currentParentIndex == 0 ? -1 : m_currentParentIndex; // don't return fake root

        public bool TryGetSection(long sectionId, out IProfiler.Section section)
        {
            if (sectionId <= 0 || sectionId >= m_allSections.Count)
            {
                section = default;
                return false;
            }

            section = m_allSections[(int)sectionId].data;
            return true;
        }

        public IEnumerable<IProfiler.Section> GetSections(long parentSectionId)
        {
            CustomSection parentSection = m_allSections[parentSectionId < 0 ? 0 : (int)parentSectionId];
            int child = parentSection.firstChildIndex;
            while (child != -1)
            {
                var section = m_allSections[child];
                yield return section.data;
                child = section.nextSiblingIndex;
            }
        }

        public long GetParentSectionId(long sectionId)
        {
            if (sectionId == 0) // don't return fake root
                return -1;

            int parentId = m_allSections[(int)sectionId].parentIndex;
            if (parentId == 0) // don't return fake root
                return -1;

            return parentId;
        }
    }
}
