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

            if (newSample.progress <= m_currentSample.progress) // progress reduced or remained
            {
                // discard this sample, and update ETA text based on last changed sample and this one

                if (!m_lastChangedSample.HasValue)
                    return;

                if (newSample.progress <= m_lastChangedSample.Value.progress) // progress reduced or remained even compared to last changed sample
                    return;

                this.ETA = GetETAText(m_lastChangedSample.Value, newSample);
                return;
            }

            // progress increased

            this.ETA = GetETAText(m_currentSample, newSample);

            m_lastChangedSample = m_currentSample;
            m_currentSample = newSample;
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
