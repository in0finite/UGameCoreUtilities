using UnityEngine;

namespace UGameCore.Utilities
{
    public class ETAMeasurer
    {
        private struct Sample
        {
            public double time;
            public float progress;
        }

        readonly System.Diagnostics.Stopwatch m_totalTimeStopwatch = System.Diagnostics.Stopwatch.StartNew();

        Sample? m_lastChangedSample;
        Sample m_currentSample;

        readonly float m_sampleInterval = 1f;
        double m_timeWhenSampled = 0;

        public string ETA { get; private set; } = F.FormatElapsedTime(0);


        public ETAMeasurer(float sampleInterval)
        {
            if (float.IsNaN(sampleInterval) || sampleInterval < 0f)
                throw new System.ArgumentOutOfRangeException(nameof(sampleInterval));

            m_sampleInterval = sampleInterval;
        }

        public void UpdateETA(float newProgressPerc)
        {
            if (float.IsNaN(newProgressPerc))
                return;

            newProgressPerc = Mathf.Clamp01(newProgressPerc);

            double timeNow = m_totalTimeStopwatch.Elapsed.TotalSeconds;

            double elapsedSeconds = timeNow - m_timeWhenSampled;

            if (elapsedSeconds < m_sampleInterval)
                return;

            m_timeWhenSampled = timeNow;

            Sample newSample = new Sample { time = timeNow, progress = newProgressPerc };
            Sample previousSample = m_lastChangedSample ?? new Sample(); // if last sample does not exist, use 'starting' sample

            if (TryUpdateETAText(previousSample, m_currentSample, newSample, out string newEtaText))
                this.ETA = newEtaText;

            // only assign new sample if progress changed
            // assign it even if progress reduced - this will support processes that can revert their progress back
            if (newSample.progress != m_currentSample.progress)
                this.AssignNewSample(newSample);
        }

        private void AssignNewSample(Sample newSample)
        {
            m_lastChangedSample = m_currentSample;
            m_currentSample = newSample;
        }

        private static bool TryUpdateETAText(Sample previousSample, Sample currentSample, Sample newSample, out string etaText)
        {
            if (newSample.progress > currentSample.progress)
            {
                // progress increased
                etaText = GetETAText(currentSample, newSample);
                return true;
            }

            if (newSample.progress > previousSample.progress)
            {
                // progress increased compared to previous sample
                etaText = GetETAText(previousSample, newSample);
                return true;
            }

            // progress reduced or remained same
            etaText = null;
            return false;
        }

        static string GetETAText(Sample previousSample, Sample currentSample)
        {
            double progressDiff = currentSample.progress - previousSample.progress;
            double timeDiff = currentSample.time - previousSample.time;
            double progressPerSecond = progressDiff / timeDiff;

            double progressLeft = 1.0 - currentSample.progress;
            double secondsLeft = progressLeft / progressPerSecond;
            return F.FormatElapsedTime(secondsLeft);
        }
    }
}
