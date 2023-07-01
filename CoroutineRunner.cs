using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UGameCore.Utilities
{
    public sealed class CoroutineInfo
    {
        private static long s_lastId = 0;
        public long Id { get; } = ++s_lastId;

        public IEnumerator coroutine { get; }
        public System.Action onFinishSuccess { get; }
        public System.Action<System.Exception> onFinishError { get; }
        public event System.Action<System.Exception> onFinish = delegate { };

        public bool IsRunning { get; internal set; } = true;

        internal CoroutineInfo(Func<IEnumerator> coroutineFunc, Action onFinishSuccess, Action<Exception> onFinishError)
        {
            this.coroutine = new NestingEnumerator(coroutineFunc, false);
            this.onFinishSuccess = onFinishSuccess;
            this.onFinishError = onFinishError;
        }

        internal void NotifyOnFinish(System.Exception ex) => this.onFinish.InvokeEventExceptionSafe(ex);
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

            int index = m_coroutines.IndexOf(coroutineInfo);
            if (index >= 0)
            {
                coroutineInfo.IsRunning = false;
                m_coroutines[index] = null;
            }

            m_newCoroutines.Remove(coroutineInfo);
        }

        public bool IsCoroutineRunning(CoroutineInfo coroutineInfo)
        {
            if (null == coroutineInfo)
                return false;

            return coroutineInfo.IsRunning;
        }

        public void Update()
        {
            m_coroutines.RemoveAll(c => null == c || !c.IsRunning);

            m_coroutines.AddRange(m_newCoroutines);
            m_newCoroutines.Clear();

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
            }
        }
    }

    public class NestingEnumerator : IEnumerator
    {
        readonly IEnumerator m_startingEnumerator;
        object m_current;
        readonly Stack<IEnumerator> m_stack = new Stack<IEnumerator>();
        readonly bool m_noExceptions;


        public NestingEnumerator(Func<IEnumerator> enumeratorFunc, bool noExceptions)
        {
            m_noExceptions = noExceptions;

            m_startingEnumerator = GetEnumeratorSafe(enumeratorFunc, m_noExceptions);
            
            if (m_startingEnumerator != null)
                m_stack.Push(m_startingEnumerator);
        }

        object IEnumerator.Current => m_current;

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

        bool IEnumerator.MoveNext()
        {
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

        void IEnumerator.Reset()
        {
            if (m_startingEnumerator != null)
                ResetSafe(m_startingEnumerator, m_noExceptions);

            m_stack.Clear();

            if (m_startingEnumerator != null)
                m_stack.Push(m_startingEnumerator);

            m_current = null;
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
