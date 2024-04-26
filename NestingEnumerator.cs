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
        public Exception FailureException { get; private set; }


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

            if (!ValidateCurrentObjectSafe(current, catchExceptions))
            {
                m_stack.Pop();
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

            m_startingEnumerator = this.GetEnumeratorSafe(tempFunc, m_noExceptions);

            if (m_startingEnumerator != null)
                m_stack.Push(m_startingEnumerator);
        }

        public bool MoveNext()
        {
            this.Initialize();

            while (true)
            {
                bool? bMoveNext = null;

                if (m_noExceptions)
                {
                    try
                    {
                        bMoveNext = MoveNextOneIteration();
                    }
                    catch (Exception ex)
                    {
                        this.ProcessException(ex);
                        return false;
                    }
                }
                else
                {
                    bMoveNext = MoveNextOneIteration();
                }

                if (bMoveNext.HasValue)
                    return bMoveNext.Value;
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
            this.FailureException = null;
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
            this.FailureException = null;
            m_stack.Clear();

            m_enumeratorFunc = func;
        }

        void ProcessException(Exception exception)
        {
            this.FailureException = exception;

            // no exception should leave this function
            try
            {
                UnityEngine.Debug.LogException(exception);
            }
            catch
            {
            }
        }

        // "safe" methods

        IEnumerator GetEnumeratorSafe(Func<IEnumerator> func, bool catchExceptions)
        {
            if (!catchExceptions)
                return func();

            IEnumerator enumerator = null;

            try
            {
                enumerator = func();
            }
            catch (Exception ex)
            {
                this.ProcessException(ex);
            }
            
            return enumerator;
        }

        bool MoveNextSafe(IEnumerator enumerator, bool catchExceptions)
        {
            if (!catchExceptions)
                return enumerator.MoveNext();

            bool hasNext = false;

            try
            {
                hasNext = enumerator.MoveNext();
            }
            catch (Exception ex)
            {
                this.ProcessException(ex);
            }

            return hasNext;
        }

        object GetCurrentSafe(IEnumerator enumerator, bool catchExceptions)
        {
            if (!catchExceptions)
                return enumerator.Current;

            object current = null;

            try
            {
                current = enumerator.Current;
            }
            catch (Exception ex)
            {
                this.ProcessException(ex);
            }

            return current;
        }

        bool ValidateCurrentObjectSafe(object currentObj, bool catchExceptions)
        {
            if (currentObj == null)
                return true;

            if (currentObj is IEnumerator)
                return true;

            var ex = new InvalidOperationException($"Invalid object type returned from IEnumerator: {currentObj.GetType()}");

            if (!catchExceptions)
                throw ex;

            ProcessException(ex);

            return false;
        }

        void ResetSafe(IEnumerator enumerator, bool catchExceptions)
        {
            if (!catchExceptions)
            {
                enumerator.Reset();
                return;
            }

            try
            {
                enumerator.Reset();
            }
            catch (Exception ex)
            {
                this.ProcessException(ex);
            }
        }

        class EnumeratorInfo
        {
            public Func<IEnumerator> enumFunc;
            public IEnumerator enumerator;
            public EnumeratorInfo parent;
            public object current;

            public EnumeratorInfo(Func<IEnumerator> enumFunc, IEnumerator enumerator, EnumeratorInfo parent, object current)
            {
                this.enumFunc = enumFunc;
                this.enumerator = enumerator;
                this.parent = parent;
                this.current = current;
            }
        }

        public static IEnumerator EnumerateInParallel(List<Func<IEnumerator>> enumFuncs)
        {
            if (enumFuncs.Count == 0)
                yield break;

            var wantWorkEnumerators = new Stack<EnumeratorInfo>(enumFuncs.Count);
            var wantPauseEnumerators = new Queue<EnumeratorInfo>();

            for (int i = enumFuncs.Count - 1; i >= 0; i--)
                wantWorkEnumerators.Push(new EnumeratorInfo(enumFuncs[i], null, null, null));
            
            while (wantWorkEnumerators.Count > 0 || wantPauseEnumerators.Count > 0)
            {
                bool shouldYieldCurrent = wantWorkEnumerators.Count == 0;

                EnumeratorInfo enumInfo = wantWorkEnumerators.Count > 0
                    ? wantWorkEnumerators.Pop()
                    : wantPauseEnumerators.Dequeue();

                if (shouldYieldCurrent)
                    yield return enumInfo.current;

                enumInfo.enumerator ??= enumInfo.enumFunc();

                IEnumerator en = enumInfo.enumerator;

                if (en.MoveNext())
                {
                    object current = en.Current;

                    if (current is IEnumerator nestedEnumerator) // he wants to continue working
                    {
                        wantWorkEnumerators.Push(new EnumeratorInfo(null, nestedEnumerator, enumInfo, null));
                    }
                    else // he wants to pause
                    {
                        enumInfo.current = current;
                        wantPauseEnumerators.Enqueue(enumInfo);
                    }
                }
                else
                {
                    // go to parent
                    if (enumInfo.parent != null)
                        wantWorkEnumerators.Push(enumInfo.parent);
                }
            }
        }
    }
}
