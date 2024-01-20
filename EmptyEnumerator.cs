using System;
using System.Collections;

namespace UGameCore.Utilities
{
    public class EmptyEnumerator : IEnumerator
    {
        public static readonly EmptyEnumerator Instance = new();

        object IEnumerator.Current => throw new InvalidOperationException("This should not have been called");

        bool IEnumerator.MoveNext() => false;

        void IEnumerator.Reset()
        {
        }
    }
}
