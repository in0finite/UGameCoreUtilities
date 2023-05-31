using System;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Forwards progress reports from <see cref="IProgress{T}"/> to <see cref="IProgressNotifier"/>.
    /// </summary>
    public class ProgressForwarder<T> : IProgress<T>
    {
        public IProgressNotifier ProgressNotifier { get; }

        public ProgressForwarder(IProgressNotifier progressNotifier) => ProgressNotifier = progressNotifier;

        public void Report(T value) => this.ProgressNotifier.SetProgress(value.ToString());
    }
}
