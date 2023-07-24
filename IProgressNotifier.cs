namespace UGameCore.Utilities
{
    /// <summary>
    /// Interface that can be used to notify about progress on long running operations, as well
    /// as to calculate ETA.
    /// </summary>
    public interface IProgressNotifier
    {
        /// <summary>
        /// Is progress currently assigned or cleared ?
        /// </summary>
        public bool HasProgress { get; }

        /// <summary>
        /// Last assigned progress.
        /// </summary>
        public float Progress { get; }

        /// <summary>
        /// Last assigned progress title.
        /// </summary>
        public string ProgressTitle { get; }

        /// <summary>
        /// Last assigned progress description.
        /// </summary>
        public string ProgressDescription { get; }

        /// <summary>
        /// Update the progress. The method can throw exception if the operation is cancelled by user.
        /// </summary>
        /// <exception cref="System.OperationCanceledException"></exception>
        void SetProgress(string title, string description = null, float? progress = null);

        /// <summary>
        /// Clear the current progress, along with any visual information about it. For example,
        /// if dialog is being shown about current progress, it will be closed.
        /// </summary>
        void ClearProgress();

        /// <summary>
        /// ETA calculated based on previous progress updates.
        /// </summary>
        string ETAText { get; }

        protected class DisposableProgress : System.IDisposable
        {
            bool m_disposed = false;
            public IProgressNotifier ProgressNotifier { get; set; }

            void System.IDisposable.Dispose()
            {
                if (m_disposed)
                    return;
                m_disposed = true;
                this.ProgressNotifier?.ClearProgress();
            }
        }

        /// <summary>
        /// Set progress that will be cleared when returned object is disposed.
        /// </summary>
        System.IDisposable SetDisposableProgress(string title, string info = null, float? progress = null)
        {
            this.SetProgress(title, info, progress);
            return new DisposableProgress { ProgressNotifier = this };
        }
    }
}
