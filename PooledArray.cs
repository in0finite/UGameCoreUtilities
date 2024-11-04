using System;
using System.Buffers;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Disposable array rented from <see cref="ArrayPool{T}"/>.
    /// </summary>
    public struct PooledArray<T> : IDisposable
    {
        T[] m_array;
        int m_minArraySize;
        ArrayPool<T> m_arrayPool;

        /// <summary>
        /// Minimum array length that was requested.
        /// </summary>
        public readonly int Length => m_minArraySize;


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

        /// <summary>
        /// Span over the rented array, with exact size that was requested.
        /// </summary>
        public readonly Span<T> Span => m_array.AsSpan(0, m_minArraySize);

        /// <summary>
        /// Array that was rented from <see cref="ArrayPool{T}"/>.
        /// Note that this array can be larger than requested size.
        /// </summary>
        public readonly T[] Array => m_array;
    }
}
