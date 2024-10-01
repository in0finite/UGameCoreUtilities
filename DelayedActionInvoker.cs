using System;
using System.Collections.Generic;
using UnityEngine;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Allows actions to be registered which will be invoked later.
    /// </summary>
    public class DelayedActionInvoker : MonoBehaviour
    {
        IGameTimeProvider m_gameTimeProvider;

        public class PerListData
        {
            // cached minimum time
            public double minTime = double.PositiveInfinity;

            // An ideal collection here would be PriorityQueue, but since we don't have it, the next best one
            // is LinkedList.

            // SortedSet/SortedDictionary allocates at least O(log(n)) memory for every operation (insert, remove, iterate).

            // SortedList would be very slow, because it has to move entire array for insert/remove.

            // List would be slow at inserting (binary search O(log(n)) + move entire array O(n)), everything else would be fine.

            // Note: we potentially have to remove multiple elements every frame.

            // With LinkedList, there is no memory allocation, except when adding elements.
            // Insert sorted O(n), Remove O(1), Iterate O(n).

            public readonly LinkedList<(double time, Action action)> actions = new();

            public readonly List<(double time, Action action)> newActions = new();
        }

        public struct Period
        {
            public double start;
            public double end;

            public Period(double start, double end)
            {
                this.start = start;
                this.end = end;
            }
        }

        readonly PerListData m_absoluteTimeList = new();
        readonly PerListData m_elapsedTimeList = new();

        double m_elapsedTime = 0;

        readonly LinkedList<(Period period, Action action)> m_outsideOfPeriodActions = new();
        readonly List<(Period period, Action action)> m_newOutsideOfPeriodActions = new();



        DelayedActionInvoker()
        {
            EditorApplicationEvents.Register(this);
        }

        void Awake()
        {
            m_gameTimeProvider = this.GetSingleComponentOrThrow<IGameTimeProvider>();
        }

        void Update()
        {
            double currentTime = m_gameTimeProvider.Time;

            this.UpdateList(m_absoluteTimeList, currentTime);

            m_elapsedTime += m_gameTimeProvider.DeltaTime;
            this.UpdateList(m_elapsedTimeList, m_elapsedTime);

            this.UpdateOutsideOfPeriodList(currentTime);
        }

        void UpdateList(PerListData perListData, double currentTime)
        {
            foreach (var pair in perListData.newActions)
                this.Add(perListData, pair.time, pair.action);

            perListData.newActions.Clear();

            if (perListData.actions.Count == 0)
                return;

            if (currentTime < perListData.minTime)
                return;

            int numExecuted = 0;
            var node = perListData.actions.First;

            while (node != null)
            {
                if (node.Value.time > currentTime)
                    break;

                numExecuted++;

                var nextNode = node.Next;

                F.RunExceptionSafe(node.Value.action);
                perListData.actions.Remove(node);
                
                node = nextNode;
            }

            if (numExecuted == 0)
                throw new ShouldNotHappenException();

            // cache min time again
            double oldMinTime = perListData.minTime;
            var smallestNode = perListData.actions.First;
            if (smallestNode != null)
                perListData.minTime = smallestNode.Value.time;
            else
                perListData.minTime = double.PositiveInfinity;

            if (perListData.minTime < oldMinTime)
                throw new ShouldNotHappenException();
        }

        void UpdateOutsideOfPeriodList(double currentTime)
        {
            foreach (var a in m_newOutsideOfPeriodActions)
                m_outsideOfPeriodActions.AddLast(a);
            
            m_newOutsideOfPeriodActions.Clear();

            var node = m_outsideOfPeriodActions.First;

            while (node != null)
            {
                var next = node.Next;

                Period period = node.Value.period;

                if (currentTime < period.start || currentTime > period.end)
                {
                    F.RunExceptionSafe(node.Value.action);
                    m_outsideOfPeriodActions.Remove(node);
                }

                node = next;
            }
        }

        void Add(PerListData perListData, double time, Action action)
        {
            // cache min time
            if (time < perListData.minTime)
                perListData.minTime = time;

            perListData.actions.InsertSorted((time, action), static (a, b) => a.time.CompareTo(b.time));
        }

        void CheckTimeArgument(double time)
        {
            if (double.IsNaN(time))
                throw new ArgumentException("Time is Nan");
            if (double.IsInfinity(time))
                throw new ArgumentException("Time is Infinity");
            if (time < 0)
                throw new ArgumentException("Time is negative");
        }

        public void RunAtTime(double time, Action action)
        {
            this.CheckTimeArgument(time);

            m_absoluteTimeList.newActions.Add((time, action));
        }

        /// <summary>
        /// Run after specified time elapsed, ignoring absolute (timeline) time, only counting delta time.
        /// </summary>
        public void RunAfterElapsed(double deltaTime, Action action)
        {
            this.CheckTimeArgument(deltaTime);

            m_elapsedTimeList.newActions.Add((m_elapsedTime + deltaTime, action));
        }

        public void RunOutsideOfPeriod(double periodStart, double periodEnd, Action action)
        {
            this.CheckTimeArgument(periodStart);
            this.CheckTimeArgument(periodEnd);

            m_newOutsideOfPeriodActions.Add((new Period(periodStart, periodEnd), action));
        }

        public void RunOutsideOfCurrentPeriod(double currentPeriodDuration, Action action)
        {
            double timeNow = m_gameTimeProvider.Time;
            this.RunOutsideOfPeriod(timeNow, timeNow + currentPeriodDuration, action);
        }
    }
}
