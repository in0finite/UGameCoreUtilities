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

            // Sorted Set/Dictionary allocates at least log(n) memory for every operation (insert, remove, iterate).

            // SortedList would be very slow, because it has to move entire array for insert/remove.

            // Keep in mind that we potentially have to remove multiple elements every frame.

            // With LinkedList, there is no memory allocation, except when adding elements.

            public readonly LinkedList<(double time, Action action)> actions = new();

            public readonly List<(double time, Action action)> newActions = new();
        }

        readonly PerListData m_absoluteTimeList = new();
        readonly PerListData m_elapsedTimeList = new();

        double m_elapsedTime = 0;



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
            this.UpdateList(m_absoluteTimeList, m_gameTimeProvider.Time);

            m_elapsedTime += m_gameTimeProvider.DeltaTime;
            this.UpdateList(m_elapsedTimeList, m_elapsedTime);
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

        void Add(PerListData perListData, double time, Action action)
        {
            // cache min time
            if (time < perListData.minTime)
                perListData.minTime = time;

            perListData.actions.InsertSorted((time, action), (a, b) => a.time.CompareTo(b.time));
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
    }
}
