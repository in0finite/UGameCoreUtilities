using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UGameCore.Utilities
{
    public sealed class CoroutineInfo
    {
        private static long s_lastId = 0; // not thread-safe
        public long Id { get; } = ++s_lastId;

        internal IEnumerator coroutine;
        internal System.Action onFinishSuccess;
        internal System.Action<System.Exception> onFinishError;
        public event System.Action<System.Exception> onFinish = delegate { };

        public bool IsRunning { get; internal set; } = true;

        /// <summary>
        /// True if coroutine reached end without throwing exception and without being manually stopped.
        /// </summary>
        public bool FinishedSuccessfully { get; internal set; } = false;

        /// <summary>
        /// Exception that caused this coroutine to fail. Note that coroutine can be stopped manually. In this case,
        /// exception will be null, but you can still check for success using <see cref="FinishedSuccessfully"/>.
        /// </summary>
        public Exception FailureException { get; internal set; }


        internal CoroutineInfo(Func<IEnumerator> coroutineFunc, Action onFinishSuccess, Action<Exception> onFinishError)
        {
            this.coroutine = new NestingEnumerator(coroutineFunc, false);
            this.onFinishSuccess = onFinishSuccess;
            this.onFinishError = onFinishError;
        }

        internal void NotifyOnFinish(System.Exception ex) => this.onFinish.InvokeEventExceptionSafe(ex);

        internal void ReleaseReferences()
        {
            this.coroutine = null;
            this.onFinishSuccess = null;
            this.onFinishError = null;
            this.onFinish = null;
        }
    }

    public sealed class CoroutineRunner
    {
        private List<CoroutineInfo> m_coroutines = new List<CoroutineInfo>();
        private List<CoroutineInfo> m_newCoroutines = new List<CoroutineInfo>();


        public CoroutineInfo StartCoroutine(Func<IEnumerator> coroutineFunc, System.Action onFinishSuccess = null, System.Action<System.Exception> onFinishError = null)
        {
            var coroutineInfo = new CoroutineInfo(coroutineFunc, onFinishSuccess, onFinishError);
            m_newCoroutines.Add(coroutineInfo);
            return coroutineInfo;
        }

        public CoroutineInfo StartCoroutine(IEnumerator coroutine, System.Action onFinishSuccess = null, System.Action<System.Exception> onFinishError = null)
            => this.StartCoroutine(() => coroutine, onFinishSuccess, onFinishError);

        public void StopCoroutine(CoroutineInfo coroutineInfo)
        {
            if (null == coroutineInfo)
                return;

            coroutineInfo.IsRunning = false;
            coroutineInfo.ReleaseReferences();
        }

        public bool IsCoroutineRunning(CoroutineInfo coroutineInfo)
        {
            if (null == coroutineInfo)
                return false;

            return coroutineInfo.IsRunning;
        }

        public void Update()
        {
            m_coroutines.AddRange(m_newCoroutines);
            m_newCoroutines.Clear();

            m_coroutines.RemoveAll(c => null == c || !c.IsRunning);

            for (int i = 0; i < m_coroutines.Count; i++)
            {
                this.UpdateCoroutine(m_coroutines[i], i);
            }
        }

        void UpdateCoroutine(CoroutineInfo coroutine, int coroutineIndex)
        {
            bool isFinished = false;
            bool isSuccess = false;
            System.Exception failureException = null;

            try
            {
                if (!coroutine.coroutine.MoveNext())
                {
                    isFinished = true;
                    isSuccess = true;
                }
            }
            catch (System.Exception ex)
            {
                isFinished = true;
                isSuccess = false;
                failureException = ex;
                Debug.LogException(ex);
            }

            if (isFinished)
            {
                coroutine.IsRunning = false;
                coroutine.FinishedSuccessfully = isSuccess;
                coroutine.FailureException = failureException;

                m_coroutines[coroutineIndex] = null;

                coroutine.NotifyOnFinish(failureException);

                if (isSuccess)
                {
                    if (coroutine.onFinishSuccess != null)
                        F.RunExceptionSafe(coroutine.onFinishSuccess);
                }
                else
                {
                    if (coroutine.onFinishError != null)
                        F.RunExceptionSafe(() => coroutine.onFinishError(failureException));
                }

                // coroutine object can still be referenced externally, so make sure to release
                // references that are no longer needed
                coroutine.ReleaseReferences();
            }
        }
    }

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
