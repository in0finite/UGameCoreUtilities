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
    }
}
