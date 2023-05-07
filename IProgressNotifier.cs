namespace UGameCore.Utilities
{
    /// <summary>
    /// Interface that can be used to notify about progress on long running operations, as well
    /// as to calculate ETA.
    /// </summary>
    public interface IProgressNotifier
    {
        /// <summary>
        /// Is progress currently assigned ?
        /// </summary>
        public bool HasProgress { get; }

        /// <summary>
        /// Update the progress. The method can throw exception if the operation is cancelled by user.
        /// </summary>
        /// <exception cref="System.OperationCanceledException"></exception>
        void SetProgress(string title, string description = null, float progressPercentage = 0f);

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
            public IProgressNotifier ProgressNotifier { get; set; }

            void System.IDisposable.Dispose()
            {
                this.ProgressNotifier?.ClearProgress();
            }
        }

        /// <summary>
        /// Set progress that will be cleared when IDisposable is disposed.
        /// </summary>
        System.IDisposable SetDisposableProgress(string title, string info = null, float progress = 0f)
        {
            this.SetProgress(title, info, progress);
            return new DisposableProgress { ProgressNotifier = this };
        }
    }
}
