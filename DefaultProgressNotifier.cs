using UnityEngine;

namespace UGameCore.Utilities
{
    public class DefaultProgressNotifier : IProgressNotifier
    {
        public IProfiler Profiler { get; private set; }
        protected ETAMeasurer m_ETAMeasurer = new ETAMeasurer(0f);
        public bool ShowEditorProgressBarInPlayMode { get; set; } = true;

        public bool HasProgress { get; protected set; } = false;

        public float Progress { get; protected set; } = 0f;

        public string ProgressTitle { get; protected set; } = string.Empty;



        public DefaultProgressNotifier(IProfiler profiler)
        {
            this.Profiler = profiler;
        }

        public virtual void SetProgress(string title, string description, float? progress)
        {
            if (progress.HasValue)
                this.Progress = progress.Value;

            this.ProgressTitle = title;

#if UNITY_EDITOR
            if (this.ShowEditorProgressBarInPlayMode || !Application.isPlaying)
            {
                if (UnityEditor.EditorUtility.DisplayCancelableProgressBar(title, description, this.Progress))
                {
                    this.HasProgress = true;
                    this.ClearProgress();
                    throw new System.OperationCanceledException($"Operation cancellled by user by clicking on Editor dialog");
                }
            }
#endif

            this.HasProgress = true;

            m_ETAMeasurer.UpdateETA(this.Progress);

            this.Profiler?.EndSection();
            this.Profiler?.BeginSection(title);
        }

        public virtual void ClearProgress()
        {
            if (!this.HasProgress)
                return;

            this.HasProgress = false;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.ClearProgressBar();
#endif

            m_ETAMeasurer = new ETAMeasurer(0f);
        }

        public virtual string ETAText => m_ETAMeasurer.ETA;
    }
}
