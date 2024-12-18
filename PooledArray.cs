using System;
using System.Buffers;
using System.Runtime.CompilerServices;

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

        static readonly bool IsUnmanagedType = !RuntimeHelpers.IsReferenceOrContainsReferences<T>();

        /// <summary>
        /// Minimum array length that was requested.
        /// </summary>
        public readonly int Length => m_minArraySize;


        /// <summary>
        /// Create empty <see cref="PooledArray{T}"/>, without renting array from <see cref="ArrayPool{T}"/>, 
        /// which can be resized later if needed.
        /// </summary>
        public static PooledArray<T> Empty() => new PooledArray<T>(0);

        /// <summary>
        /// Create empty <see cref="PooledArray{T}"/>, without renting array from <see cref="ArrayPool{T}"/>, 
        /// which can be resized later if needed.
        /// </summary>
        public static PooledArray<T> Empty(ArrayPool<T> arrayPool) => new PooledArray<T>(0, arrayPool);


        public PooledArray(int minArraySize, ArrayPool<T> arrayPool)
        {
            if (arrayPool == null)
                throw new ArgumentNullException(nameof(arrayPool));
            if (minArraySize < 0)
                throw new ArgumentOutOfRangeException(nameof(minArraySize));

            m_arrayPool = arrayPool;
            m_minArraySize = minArraySize;
            m_array = null;
            InitializeArray();
        }

        public PooledArray(int minArraySize)
            : this(minArraySize, ArrayPool<T>.Shared)
        {
        }

        void InitializeArray()
        {
            m_array = m_minArraySize == 0 ? System.Array.Empty<T>() : m_arrayPool.Rent(m_minArraySize);
        }

        public void Dispose()
        {
            ReturnArray(m_array);

            m_array = null;
            m_arrayPool = null;
            m_minArraySize = 0;
        }

        readonly void ReturnArray(T[] arr)
        {
            if (arr != null && arr.Length > 0 && m_arrayPool != null)
            {
                // Clear references from array, so that GC can immediatelly collect them.
                // Otherwise they will remain in ArrayPool until he disposes the array.
                if (!IsUnmanagedType)
                    System.Array.Clear(arr, 0, arr.Length);

                m_arrayPool.Return(arr);
            }
        }

        /// <summary>
        /// Resize the array if needed. If new size is higher than current capacity (array's length), current 
        /// array will be returned to <see cref="ArrayPool{T}"/> and new array will be rented.
        /// </summary>
        public void Resize(int newMinSize)
        {
            if (newMinSize < 0)
                throw new ArgumentOutOfRangeException();

            if (m_arrayPool == null) // can happen if variable is not initialized, or initialized with `default`
                throw new InvalidOperationException("ArrayPool is null");

            int currentCapacity = m_array.Length;

            if (newMinSize <= currentCapacity)
            {
                // no need to resize
                m_minArraySize = newMinSize;
                return;
            }

            // new size is higher than capacity, resize

            T[] oldArray = m_array;
            m_minArraySize = newMinSize;
            InitializeArray();

            oldArray.CopyTo(m_array, 0);

            // only AFTER we copied data from old array, we can return it to ArrayPool
            ReturnArray(oldArray);
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
