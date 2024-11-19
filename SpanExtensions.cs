using System;
using System.Runtime.InteropServices;

namespace UGameCore.Utilities
{
    public static class SpanExtensions
    {
        public static Span<TTo> CastWithSameLength<TFrom, TTo>(this Span<TFrom> span)
            where TFrom : struct
            where TTo : struct
        {
            Span<TTo> result = MemoryMarshal.Cast<TFrom, TTo>(span);
            if (span.Length != result.Length)
                throw new InvalidOperationException($"Failed to cast Span<{typeof(TFrom).Name}> to Span<{typeof(TTo).Name}>, with same length");
            return result;
        }

        public static ReadOnlySpan<TTo> CastWithSameLength<TFrom, TTo>(this ReadOnlySpan<TFrom> span)
            where TFrom : struct
            where TTo : struct
        {
            ReadOnlySpan<TTo> result = MemoryMarshal.Cast<TFrom, TTo>(span);
            if (span.Length != result.Length)
                throw new InvalidOperationException($"Failed to cast Span<{typeof(TFrom).Name}> to Span<{typeof(TTo).Name}>, with same length");
            return result;
        }

        public static ReadOnlySpan<TTo> CastEntirely<TFrom, TTo>(this ReadOnlySpan<TFrom> span)
            where TFrom : struct
            where TTo : struct
        {
            ReadOnlySpan<TTo> result = MemoryMarshal.Cast<TFrom, TTo>(span);

            int sizeOfFrom = Marshal.SizeOf<TFrom>();
            int sizeOfTo = Marshal.SizeOf<TTo>();

            if (span.Length * sizeOfFrom != result.Length * sizeOfTo)
                throw new InvalidOperationException($"Failed to cast Span<{typeof(TFrom).Name}> to Span<{typeof(TTo).Name}> entirely");

            return result;
        }

        public static Span<TTo> CastEntirely<TFrom, TTo>(this Span<TFrom> span)
            where TFrom : struct
            where TTo : struct
        {
            Span<TTo> result = MemoryMarshal.Cast<TFrom, TTo>(span);

            int sizeOfFrom = Marshal.SizeOf<TFrom>();
            int sizeOfTo = Marshal.SizeOf<TTo>();

            if (span.Length * sizeOfFrom != result.Length * sizeOfTo)
                throw new InvalidOperationException($"Failed to cast Span<{typeof(TFrom).Name}> to Span<{typeof(TTo).Name}> entirely");

            return result;
        }

        public static Span<T> CopyFromOther<T>(this Span<T> destinationSpan, ReadOnlySpan<T> other)
        {
            if (destinationSpan.Length < other.Length)
                throw new ArgumentOutOfRangeException($"Destination span is shorter ({destinationSpan.Length}) than source span ({other.Length})");
            other.CopyTo(destinationSpan);
            return destinationSpan.Slice(other.Length);
        }

        public static void CopyFromOther<T>(this Span<T> destinationSpan, ReadOnlySpan<T> other, out int otherLength)
        {
            destinationSpan.CopyFromOther(other);
            otherLength = other.Length;
        }

        public static void Split<T>(this Span<T> span, T splitter, Span<int> indexes, out int numIndexes)
            where T : IEquatable<T> // Span<IEquatable<T>> has additional extension methods
        {
            int globalIndex = 0;
            numIndexes = 0;
            var startingSpan = span;

            while (true)
            {
                int localIndex = span.IndexOf(splitter);
                if (localIndex < 0)
                    break;

                globalIndex += localIndex;

                indexes[numIndexes] = globalIndex;
                numIndexes++;

                globalIndex++;

                span = startingSpan[globalIndex..];
            }
        }
    }
}
