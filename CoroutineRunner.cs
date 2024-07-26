using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UGameCore.Utilities
{
    public sealed class CoroutineInfo
    {
        internal NestingEnumerator NestingEnumerator;
        internal Func<IEnumerator> CoroutineFunc; // only used for RestartOnFailure
        internal Action OnFinishSuccess;
        internal Action<Exception> OnFinishError;
        public event Action<Exception> onFinish = delegate { };

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

        public string ProfilerSectionName { get; internal set; }

        public bool RestartOnFailure { get; internal init; } = false;


        internal CoroutineInfo() { } // restrict Constructor to internal use

        internal void NotifyOnFinish(Exception ex) => this.onFinish.InvokeEventExceptionSafe(ex);

        internal void ReleaseReferences()
        {
            this.NestingEnumerator = null;
            this.CoroutineFunc = null;
            this.OnFinishSuccess = null;
            this.OnFinishError = null;
            this.onFinish = null;
            this.ProfilerSectionName = null;
        }
    }

    public sealed class CoroutineRunner
    {
        readonly List<CoroutineInfo> m_coroutines = new();
        readonly List<CoroutineInfo> m_newCoroutines = new();


        public CoroutineInfo StartCoroutine(
            Func<IEnumerator> coroutineFunc,
            Action onFinishSuccess = null,
            Action<Exception> onFinishError = null,
            Action<CoroutineInfo> onFinish = null,
            string profilerSectionName = null,
            bool startImmediatelly = false,
            bool restartOnFailure = false)
        {
            if (coroutineFunc == null)
                throw new ArgumentNullException(nameof(coroutineFunc));

            var coroutineInfo = new CoroutineInfo()
            {
                NestingEnumerator = new NestingEnumerator(coroutineFunc, false),
                CoroutineFunc = restartOnFailure ? coroutineFunc : null,
                OnFinishSuccess = onFinishSuccess,
                OnFinishError = onFinishError,
                ProfilerSectionName = profilerSectionName,
                RestartOnFailure = restartOnFailure,
            };

            if (onFinish != null)
                coroutineInfo.onFinish += (ex) => onFinish(coroutineInfo);
            
            if (startImmediatelly)
                this.UpdateCoroutine(coroutineInfo, -1);

            if (coroutineInfo.IsRunning) // if still running after first update
                m_newCoroutines.Add(coroutineInfo);

            return coroutineInfo;
        }

        public CoroutineInfo StartCoroutine(IEnumerator coroutine, Action onFinishSuccess = null, Action<Exception> onFinishError = null)
            => this.StartCoroutine(() => coroutine, onFinishSuccess, onFinishError);

        public void ReplaceCoroutineFunction(CoroutineInfo coroutineInfo, Func<IEnumerator> coroutineFunc)
        {
            // useful when you want to restart specified Coroutine, but maintain order of execution

            if (coroutineFunc == null)
                throw new ArgumentNullException();

            if (!coroutineInfo.IsRunning)
                throw new InvalidOperationException("Coroutine function can not be replaced if coroutine is not running");

            // we must not call ResetWithOtherEnumerator(), because this execution path could be coming from this very Enumerator,
            // so by clearing it's internal Stack, we could cause undefined behavior.

            //coroutineInfo.coroutine.ResetWithOtherEnumerator(coroutineFunc);

            coroutineInfo.CoroutineFunc = coroutineInfo.RestartOnFailure ? coroutineFunc : null;
            coroutineInfo.NestingEnumerator = new NestingEnumerator(coroutineFunc, coroutineInfo.NestingEnumerator.NoExceptions);
        }

        public void StopCoroutine(CoroutineInfo coroutineInfo)
        {
            if (null == coroutineInfo) // kept for legacy reasons
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
            Exception failureException = null;
            bool hasProfilerSection = !string.IsNullOrWhiteSpace(coroutine.ProfilerSectionName);

            // -------------- advance Coroutine --------------

            if (hasProfilerSection)
                UnityEngine.Profiling.Profiler.BeginSample(coroutine.ProfilerSectionName);

            try
            {
                if (!coroutine.NestingEnumerator.MoveNext())
                {
                    isFinished = true;
                    isSuccess = true;
                }
            }
            catch (Exception ex)
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

            // ----------------------------

            if (!coroutine.IsRunning) // stopped by calling StopCoroutine()
                return;

            if (isFinished && !isSuccess && coroutine.RestartOnFailure)
            {
                // restart on failure
                // invoke callbacks, and restart the coroutine
                this.InvokeCallbacks(coroutine, isSuccess, failureException);
                coroutine.NestingEnumerator = new NestingEnumerator(coroutine.CoroutineFunc, false);
                return;
            }

            if (isFinished)
            {
                coroutine.IsRunning = false;
                coroutine.FinishedSuccessfully = isSuccess;
                coroutine.FailureException = failureException;

                if (coroutineIndex >= 0)
                    m_coroutines[coroutineIndex] = null;

                this.InvokeCallbacks(coroutine, isSuccess, failureException);

                // coroutine object can still be referenced externally, so make sure to release
                // references that are no longer needed
                coroutine.ReleaseReferences();
            }
        }

        void InvokeCallbacks(CoroutineInfo coroutine, bool isSuccess, Exception failureException)
        {
            coroutine.NotifyOnFinish(failureException);

            if (isSuccess)
            {
                if (coroutine.OnFinishSuccess != null)
                    F.RunExceptionSafe(coroutine.OnFinishSuccess);
            }
            else
            {
                if (coroutine.OnFinishError != null)
                    F.RunExceptionSafe(() => coroutine.OnFinishError(failureException));
            }
        }
    }
}
