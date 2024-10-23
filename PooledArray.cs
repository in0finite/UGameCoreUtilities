using System;
using System.Buffers;

namespace UGameCore.Utilities
{
    public struct PooledArray<T> : IDisposable
    {
        T[] m_array;
        int m_minArraySize;
        ArrayPool<T> m_arrayPool;


        public PooledArray(int minArraySize, ArrayPool<T> arrayPool)
        {
            m_array = arrayPool.Rent(minArraySize);
            m_arrayPool = arrayPool;
            m_minArraySize = minArraySize;
        }

        public PooledArray(int minArraySize)
            : this(minArraySize, ArrayPool<T>.Shared)
        {
        }

        public void Dispose()
        {
            if (m_array != null)
            {
                m_arrayPool.Return(m_array);
            }

            m_array = null;
            m_arrayPool = null;
            m_minArraySize = 0;
        }

        public readonly Span<T> Span => m_array.AsSpan(0, m_minArraySize);

        public readonly T[] Array => m_array;
    }
}
