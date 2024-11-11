using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Provides equivalent functionality of <see cref="System.IO.MemoryStream"/> and <see cref="System.IO.BinaryReader"/>,
    /// but working with a <see cref="Span{byte}"/>.
    /// </summary>
    public ref struct SpanByteReadableStream
    {
        readonly ReadOnlySpan<byte> Span;
        int m_position;
        public int Position
        {
            readonly get => m_position;
            set => m_position = value;
        }

        public readonly int Length => Span.Length;
        public readonly int CountLeft => Span.Length - m_position;
        readonly ReadOnlySpan<byte> ReadingPartAsSpan => Span.Slice(m_position);


        public SpanByteReadableStream(ReadOnlySpan<byte> span)
            : this()
        {
            Span = span;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly ReadOnlySpan<T> ReadingPartAsCastedSpan<T>()
            where T : unmanaged
            => MemoryMarshal.Cast<byte, T>(ReadingPartAsSpan);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte() => Span[m_position++];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadUnmanaged<T>()
            where T : unmanaged
        {
            ReadOnlySpan<byte> span = ReadByteSpan(Unsafe.SizeOf<T>());

            // code taken from MemoryMarshal.Read<T>(ReadOnlySpan<byte> source)
            return Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(span));
        }

        public ReadOnlySpan<T> ReadCastedSpan<T>(int count)
            where T : unmanaged
        {
            ReadOnlySpan<T> castedSpan = ReadingPartAsCastedSpan<T>();

            ReadOnlySpan<T> resultSpan = castedSpan.Slice(0, count);

            m_position += Unsafe.SizeOf<T>() * count;

            return resultSpan;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> ReadByteSpan(int count)
        {
            ReadOnlySpan<byte> resultSpan = Span.Slice(m_position, count);
            m_position += count;
            return resultSpan;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUInt16() => ReadUnmanaged<ushort>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt32() => ReadUnmanaged<int>();
    }
}
