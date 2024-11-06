using System;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Protects certain code section from being re-entered.
    /// </summary>
    public class ReEntranceGuard
    {
        public struct Section : IDisposable
        {
            public ReEntranceGuard ReEntranceGuard { get; private set; }

            internal Section(ReEntranceGuard reentranceGuard)
            {
                ReEntranceGuard = reentranceGuard;
            }

            public void Dispose()
            {
                ReEntranceGuard?.ExitSection();
                ReEntranceGuard = null;
            }
        }

        public bool IsInsideSection { get; private set; } = false;


        public Section AttemptEntrance(string errorMessage)
        {
            if (IsInsideSection)
                throw new InvalidOperationException(errorMessage);

            IsInsideSection = true;
            return new Section(this);
        }

        internal void ExitSection()
        {
            if (IsInsideSection)
                IsInsideSection = false;
            else
                throw new ShouldNotHappenException("Trying to exit section, but we are not currently in a section");
        }
    }
}
