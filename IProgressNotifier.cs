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
        void ClearProgress(DisposableProgress? progress = default);

        /// <summary>
        /// ETA calculated based on previous progress updates.
        /// </summary>
        string ETAText { get; }

        public struct DisposableProgress : System.IDisposable
        {
            bool m_disposed;
            public IProgressNotifier ProgressNotifier { get; private set; }
            public long Id { get; }

            public DisposableProgress(IProgressNotifier ProgressNotifier, long id)
            {
                m_disposed = false;
                this.Id = id;
                this.ProgressNotifier = ProgressNotifier;
            }

            public void Dispose()
            {
                if (m_disposed)
                    return;
                m_disposed = true;
                this.ProgressNotifier?.ClearProgress(this);
                this.ProgressNotifier = null;
            }
        }

        /// <summary>
        /// Set progress that will be cleared when returned object is disposed.
        /// </summary>
        DisposableProgress SetDisposableProgress(string title, string info = null, float? progress = null)
        {
            this.SetProgress(title, info, progress);
            return new DisposableProgress(this, -1);
        }
    }
}
