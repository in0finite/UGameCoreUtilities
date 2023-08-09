using System.Collections.Generic;

namespace UGameCore.Utilities
{

	/// <summary>
	/// Alternative to System.Collections.Concurrent.ConcurrentQueue
	/// (It's only available in .NET 4.0 and greater)
	/// </summary>
	/// <remarks>
	/// It's a bit slow (as it uses locks), and only provides a small subset of the interface
	/// Overall, the implementation is intended to be simple & robust
	/// </remarks>
	public class ConcurrentQueue<T>
	{
		private readonly System.Object _queueLock = new System.Object();
		private readonly Queue<T> _queue = new Queue<T>();

		public void Enqueue(T item)
		{
			lock (_queueLock)
			{
				_queue.Enqueue(item);
			}
		}

		public bool TryDequeue(out T result)
		{
			lock (_queueLock)
			{
				if (_queue.Count == 0)
				{
					result = default(T);
					return false;
				}

				result = _queue.Dequeue();
				return true;
			}
		}

		public T[] DequeueAll()
		{
			lock (_queueLock)
			{
				T[] copy = _queue.ToArray();
				_queue.Clear();
				return copy;
			}
		}

		public int DequeueToQueue(Queue<T> collection, int maxNumItems)
		{
			lock (_queueLock)
			{
				int numAdded = 0;
				while (_queue.Count > 0 && numAdded < maxNumItems)
				{
					collection.Enqueue(_queue.Dequeue());
					numAdded++;
				}
				return numAdded;
			}
		}

        public int DequeueToArray(T[] array, int offset, int maxNumItems)
        {
            lock (_queueLock)
            {
                int numAdded = 0;
                while (_queue.Count > 0 && numAdded < maxNumItems)
                {
                    array[offset + numAdded] = _queue.Dequeue();
                    numAdded++;
                }
                return numAdded;
            }
        }

        public int DequeueToList(List<T> list, int maxNumItems)
        {
            lock (_queueLock)
            {
                int numAdded = 0;
                while (_queue.Count > 0 && numAdded < maxNumItems)
                {
                    list.Add(_queue.Dequeue());
                    numAdded++;
                }
                return numAdded;
            }
        }

        public int DequeueUntilCountReaches(int targetCount)
        {
            lock (_queueLock)
            {
                int numRemoved = 0;
                while (_queue.Count > targetCount)
                {
                    _queue.Dequeue();
                    numRemoved++;
                }
                return numRemoved;
            }
        }

        public void Clear()
        {
            lock (_queueLock)
            {
                _queue.Clear();
            }
        }

        public int Count
		{
			get
			{
				lock (_queueLock)
				{
					return _queue.Count;
				}
			}
		}

	}

}
