using System;
using System.Collections;
using System.Collections.Generic;

namespace UGameCore.Utilities
{
    public class EnumeratorEvent
    {
        List<Func<IEnumerator>> m_subscribers = new List<Func<IEnumerator>>();


        public void Subscribe(Func<IEnumerator> enumeratorFunc) => m_subscribers.Add(enumeratorFunc);

        public void Unsubscribe(Func<IEnumerator> enumeratorFunc) => m_subscribers.Remove(enumeratorFunc);

        public Func<IEnumerator>[] GetSubscribers() => m_subscribers.ToArray();

        public IEnumerator Invoke()
        {
            var listCopy = m_subscribers.ToArray();
            foreach (var enumeratorFunc in listCopy)
                yield return new NestingEnumerator(enumeratorFunc, false);
        }

        public IEnumerator InvokeNoException()
        {
            var listCopy = m_subscribers.ToArray();
            foreach (var enumeratorFunc in listCopy)
                yield return new NestingEnumerator(enumeratorFunc, true);
        }
    }
}
