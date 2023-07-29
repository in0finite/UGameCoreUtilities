using UnityEngine;
using static UGameCore.Utilities.IProgressNotifier;

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
        public string ProgressDescription { get; protected set; } = string.Empty;



        public DefaultProgressNotifier(IProfiler profiler)
        {
            this.Profiler = profiler;
        }

        public virtual void SetProgress(string title, string description, float? progress)
        {
            if (progress.HasValue)
                this.Progress = progress.Value;

            this.ProgressTitle = title;
            this.ProgressDescription = description;

#if UNITY_EDITOR
            if (this.ShowEditorProgressBarInPlayMode || !Application.isPlaying)
            {
                if (UnityEditor.EditorUtility.DisplayCancelableProgressBar(title, description, this.Progress))
                {
                    this.HasProgress = true;
                    this.ClearProgress(null);
                    throw new System.OperationCanceledException($"Operation cancellled by user by clicking on Editor dialog");
                }
            }
#endif

            this.HasProgress = true;

            m_ETAMeasurer.UpdateETA(this.Progress);

            this.Profiler?.BeginSection(title);
        }

        public virtual void ClearProgress(DisposableProgress disposableProgress)
        {
            if (disposableProgress != null)
                this.Profiler?.EndSectionWithChildren(disposableProgress.Id);
            
            if (!this.HasProgress)
                return;

            this.HasProgress = false;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.ClearProgressBar();
#endif

            m_ETAMeasurer = new ETAMeasurer(0f);

            if (disposableProgress == null) // function was manually called, close the current section
                this.Profiler?.EndSection();
        }

        public virtual string ETAText => m_ETAMeasurer.ETA;

        System.IDisposable IProgressNotifier.SetDisposableProgress(string title, string info, float? progress)
        {
            this.SetProgress(title, info, progress);
            return new DisposableProgress { ProgressNotifier = this, Id = this.Profiler?.CurrentSectionId ?? -1 };
        }
    }
}
