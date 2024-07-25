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
        internal NestingEnumerator coroutine;
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

        public string ProfilerSectionName { get; private set; }


        internal CoroutineInfo(
            Func<IEnumerator> coroutineFunc, Action onFinishSuccess, Action<Exception> onFinishError, string profilerSectionName)
        {
            this.coroutine = new NestingEnumerator(coroutineFunc, false);
            this.onFinishSuccess = onFinishSuccess;
            this.onFinishError = onFinishError;
            this.ProfilerSectionName = profilerSectionName;
        }

        internal void NotifyOnFinish(System.Exception ex) => this.onFinish.InvokeEventExceptionSafe(ex);

        internal void ReleaseReferences()
        {
            this.coroutine = null;
            this.onFinishSuccess = null;
            this.onFinishError = null;
            this.onFinish = null;
            this.ProfilerSectionName = null;
        }
    }

    public sealed class CoroutineRunner
    {
        private List<CoroutineInfo> m_coroutines = new List<CoroutineInfo>();
        private List<CoroutineInfo> m_newCoroutines = new List<CoroutineInfo>();


        public CoroutineInfo StartCoroutine(
            Func<IEnumerator> coroutineFunc,
            System.Action onFinishSuccess = null,
            System.Action<System.Exception> onFinishError = null,
            System.Action<CoroutineInfo> onFinish = null,
            string profilerSectionName = null,
            bool startImmediatelly = false)
        {
            var coroutineInfo = new CoroutineInfo(coroutineFunc, onFinishSuccess, onFinishError, profilerSectionName);
            if (onFinish != null)
                coroutineInfo.onFinish += (ex) => onFinish(coroutineInfo);
            
            if (startImmediatelly)
                this.UpdateCoroutine(coroutineInfo, -1);

            if (coroutineInfo.IsRunning) // if still running after first update
                m_newCoroutines.Add(coroutineInfo);

            return coroutineInfo;
        }

        public CoroutineInfo StartCoroutine(IEnumerator coroutine, System.Action onFinishSuccess = null, System.Action<System.Exception> onFinishError = null)
            => this.StartCoroutine(() => coroutine, onFinishSuccess, onFinishError);

        public void ReplaceCoroutineFunction(CoroutineInfo coroutineInfo, Func<IEnumerator> coroutineFunc)
        {
            // useful when you want to restart specified Coroutine, but maintain order of execution

            if (coroutineFunc == null)
                throw new ArgumentNullException();

            if (!coroutineInfo.IsRunning) // because all the callbacks are nullified
                throw new InvalidOperationException("Coroutine function can not be replaced if coroutine is not running");

            // we must not call ResetWithOtherEnumerator(), because this execution path could be coming from this very Enumerator,
            // so by clearing it's internal Stack, we could cause undefined behavior.

            //coroutineInfo.coroutine.ResetWithOtherEnumerator(coroutineFunc);

            coroutineInfo.coroutine = new NestingEnumerator(coroutineFunc, coroutineInfo.coroutine.NoExceptions);
        }

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
            UnityEngine.Profiling.Profiler.BeginSample("CoroutineRunner Update");

            m_newCoroutines.RemoveAll(c => null == c || !c.IsRunning);

            m_coroutines.AddRange(m_newCoroutines);
            m_newCoroutines.Clear();

            m_coroutines.RemoveAll(c => null == c || !c.IsRunning);

            for (int i = 0; i < m_coroutines.Count; i++)
            {
                this.UpdateCoroutine(m_coroutines[i], i);
            }

            UnityEngine.Profiling.Profiler.EndSample();
        }

        void UpdateCoroutine(CoroutineInfo coroutine, int coroutineIndex)
        {
            bool isFinished = false;
            bool isSuccess = false;
            System.Exception failureException = null;
            bool hasProfilerSection = !string.IsNullOrWhiteSpace(coroutine.ProfilerSectionName);

            if (hasProfilerSection)
                UnityEngine.Profiling.Profiler.BeginSample(coroutine.ProfilerSectionName);

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

                try
                {
                    Debug.LogException(ex);
                }
                catch
                {
                }
            }

            if (hasProfilerSection)
                UnityEngine.Profiling.Profiler.EndSample();

            if (isFinished)
            {
                coroutine.IsRunning = false;
                coroutine.FinishedSuccessfully = isSuccess;
                coroutine.FailureException = failureException;

                if (coroutineIndex >= 0)
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
}
