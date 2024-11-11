using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Provides equivalent functionality of <see cref="System.IO.MemoryStream"/> and <see cref="System.IO.BinaryWriter"/>,
    /// but working with a <see cref="Span{byte}"/>.
    /// </summary>
    public ref struct SpanByteWritableStream
    {
        readonly Span<byte> Span;
        int m_position;
        public int Position
        {
            readonly get => m_position;
            set => m_position = value;
        }

        int m_length;
        public readonly int Length => m_length;
        public readonly int Capacity => Span.Length;


        public SpanByteWritableStream(Span<byte> span)
            : this()
        {
            Span = span;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            m_position = 0;
            m_length = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByte(byte value)
            => WriteUnmanaged(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByteSpan(ReadOnlySpan<byte> span)
        {
            span.CopyTo(Span.Slice(m_position, span.Length));
            m_position += span.Length;
            m_length = Math.Max(m_length, m_position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSpan<T>(ReadOnlySpan<T> span)
            where T : unmanaged
            => WriteByteSpan(MemoryMarshal.AsBytes(span));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUnmanaged<T>(T value)
            where T : unmanaged
            => WriteSpan(MemoryMarshal.CreateReadOnlySpan(ref value, 1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt(int value) => WriteUnmanaged(value);
    }
}
