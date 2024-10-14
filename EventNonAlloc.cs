using System;
using System.Collections.Generic;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Provides all functionalities of a C# event, but without allocating memory in any of operations (except when
    /// resizing internal Lists).
    /// </summary>
    public class EventNonAlloc<T>
    {
        readonly List<Action<T>> Subscribers = new();
        readonly List<Action<T>> TempSubscribers = new();


        public void Subscribe(Action<T> action) => Subscribers.Add(action);
        public void SubscribeIfNotExists(Action<T> action) => Subscribers.AddIfNotExists(action);

        public static EventNonAlloc<T> operator +(EventNonAlloc<T> left, Action<T> right)
        {
            left.Subscribe(right);
            return left;
        }

        public void Unsubscribe(Action<T> action) => Subscribers.Remove(action);

        public static EventNonAlloc<T> operator -(EventNonAlloc<T> left, Action<T> right)
        {
            left.Unsubscribe(right);
            return left;
        }

        public void RemoveAllSubscribes() => Subscribers.Clear();

        public Action<T>[] GetSubscribers() => Subscribers.ToArray();

        public void RemoveAllSubscribers() => Subscribers.Clear();

        /// <summary>
        /// Invoke all subscribers. This function is not re-entrable.
        /// </summary>
        public void Invoke(T eventParameter)
        {
            TempSubscribers.Clear();
            TempSubscribers.AddRange(Subscribers);

            foreach (Action<T> action in TempSubscribers)
                action(eventParameter);

            TempSubscribers.Clear();
        }

        /// <summary>
        /// Invoke all subscribers, while catching and logging their exceptions, ensuring that every subscriber is
        /// invoked.
        /// This function is not re-entrable.
        /// </summary>
        public void InvokeNoException(T eventParameter)
        {
            TempSubscribers.Clear();
            TempSubscribers.AddRange(Subscribers);

            foreach (Action<T> action in TempSubscribers)
                F.RunExceptionSafeArg2(action, eventParameter, static (arg1, arg2) => arg1(arg2));

            TempSubscribers.Clear();
        }
    }

    public class EventNonAlloc<T1, T2> : EventNonAlloc<(T1, T2)>
    {
    }

    public class EventNonAlloc : EventNonAlloc<object>
    {
        public void Invoke() => Invoke(default);
        public void InvokeNoException() => InvokeNoException(default);
    }
}
