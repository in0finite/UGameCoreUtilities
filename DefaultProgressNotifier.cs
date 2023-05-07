namespace UGameCore.Utilities
{
    public class DefaultProgressNotifier : IProgressNotifier
    {
        protected ETAMeasurer m_ETAMeasurer = new ETAMeasurer(0f);

        public bool HasProgress { get; protected set; } = false;


        public virtual void SetProgress(string title, string description, float progressPercentage)
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorUtility.DisplayCancelableProgressBar(title, description, progressPercentage))
            {
                this.HasProgress = true;
                this.ClearProgress();
                throw new System.OperationCanceledException($"Operation cancellled by user by clicking on Editor dialog");
            }
#endif

            this.HasProgress = true;

            m_ETAMeasurer.UpdateETA(progressPercentage);
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
