using System;

namespace UGameCore.Utilities
{
    public class ShouldNotHappenException : Exception
    {
        public ShouldNotHappenException() : base("Should not happen")
        {
        }
    }
}
