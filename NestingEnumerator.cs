using System;
using System.Collections;
using System.Collections.Generic;

namespace UGameCore.Utilities
{
    public class NestingEnumerator : IEnumerator
    {
        IEnumerator m_startingEnumerator;
        Func<IEnumerator> m_enumeratorFunc;
        object m_current;
        readonly Stack<IEnumerator> m_stack = new Stack<IEnumerator>();
        readonly bool m_noExceptions;
        bool m_initialized = false;


        public NestingEnumerator(Func<IEnumerator> enumeratorFunc, bool noExceptions)
        {
            m_enumeratorFunc = enumeratorFunc;
            m_noExceptions = noExceptions;
        }

        public object Current => m_current;

        bool? MoveNextOneIteration()
        {
            if (m_stack.Count == 0)
                return false;

            bool catchExceptions = false;

            IEnumerator enumerator = m_stack.Peek();
            bool hasNext = MoveNextSafe(enumerator, catchExceptions);
            if (!hasNext)
            {
                m_stack.Pop();
                return null;
            }

            object current = GetCurrentSafe(enumerator, catchExceptions);
            if (current is IEnumerator nestedEnumerator)
            {
                m_stack.Push(nestedEnumerator);
                return null;
            }

            m_current = current;

            return true;
        }

        void Initialize()
        {
            if (m_initialized)
                return;

            m_initialized = true;

            var tempFunc = m_enumeratorFunc;
            m_enumeratorFunc = null; // we don't need it anymore, release references

            m_startingEnumerator = GetEnumeratorSafe(tempFunc, m_noExceptions);

            if (m_startingEnumerator != null)
                m_stack.Push(m_startingEnumerator);
        }

        public bool MoveNext()
        {
            this.Initialize();

            while (true)
            {
                bool? bResult = null;

                if (m_noExceptions)
                {
                    bool thrownException = !F.RunExceptionSafe(() => bResult = MoveNextOneIteration());

                    if (thrownException)
                        return false;
                }
                else
                {
                    bResult = MoveNextOneIteration();
                }

                if (bResult.HasValue)
                    return bResult.Value;
            }
        }

        public void Reset()
        {
            if (m_startingEnumerator != null)
                ResetSafe(m_startingEnumerator, m_noExceptions);

            m_stack.Clear();

            if (m_startingEnumerator != null)
                m_stack.Push(m_startingEnumerator);

            m_current = null;
        }

        /// <summary>
        /// Reset enumeration using different <see cref="IEnumerator"/>. The advantage over creating a new
        /// instance of <see cref="NestingEnumerator"/> is that you avoid 3 memory allocations (instance, stack,
        /// stack's internal array).
        /// </summary>
        public void ResetWithOtherEnumerator(Func<IEnumerator> func)
        {
            m_startingEnumerator = null;
            m_current = null;
            m_initialized = false;
            m_stack.Clear();

            m_enumeratorFunc = func;
        }

        // "safe" methods

        static IEnumerator GetEnumeratorSafe(Func<IEnumerator> func, bool catchExceptions)
        {
            if (!catchExceptions)
                return func();

            IEnumerator enumerator = null;
            F.RunExceptionSafe(() => enumerator = func());
            return enumerator;
        }

        static bool MoveNextSafe(IEnumerator enumerator, bool catchExceptions)
        {
            if (!catchExceptions)
                return enumerator.MoveNext();

            bool hasNext = false;
            F.RunExceptionSafe(() => hasNext = enumerator.MoveNext());
            return hasNext;
        }

        static object GetCurrentSafe(IEnumerator enumerator, bool catchExceptions)
        {
            if (!catchExceptions)
                return enumerator.Current;

            object current = null;
            F.RunExceptionSafe(() => current = enumerator.Current);
            return current;
        }

        static void ResetSafe(IEnumerator enumerator, bool catchExceptions)
        {
            if (!catchExceptions)
            {
                enumerator.Reset();
                return;
            }

            F.RunExceptionSafe(enumerator.Reset);
        }
    }
}
