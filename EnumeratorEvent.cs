using System;
using System.Collections;
using System.Collections.Generic;

namespace UGameCore.Utilities
{
    public class EnumeratorEvent<T>
    {
        List<Func<T, IEnumerator>> m_subscribers = new List<Func<T, IEnumerator>>();


        public void Subscribe(Func<T, IEnumerator> enumeratorFunc) => m_subscribers.Add(enumeratorFunc);

        public void Unsubscribe(Func<T, IEnumerator> enumeratorFunc) => m_subscribers.Remove(enumeratorFunc);

        public Func<T, IEnumerator>[] GetSubscribers() => m_subscribers.ToArray();

        public void RemoveAllSubscribers() => m_subscribers.Clear();

        public IEnumerator Invoke(T eventParameter)
        {
            var listCopy = m_subscribers.ToArray();
            foreach (var enumeratorFunc in listCopy)
                yield return new NestingEnumerator(() => enumeratorFunc(eventParameter), false);
        }

        public IEnumerator InvokeNoException(T eventParameter)
        {
            var listCopy = m_subscribers.ToArray();
            foreach (var enumeratorFunc in listCopy)
                yield return new NestingEnumerator(() => enumeratorFunc(eventParameter), true);
        }

        public IEnumerator InvokeNoExceptionAndClear(T eventParameter)
        {
            // note: we could return IEnumerator return from function below, but this way we make sure
            // that all subscribers are invoked, before we clear the list of subscribers
            yield return this.InvokeNoException(eventParameter);
            this.RemoveAllSubscribers();
        }
    }
}
