using System;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Provides equivalent functionality of <see cref="System.Text.StringBuilder"/>,
    /// but without memory allocations, storing all data in <see cref="Span{char}"/>.
    /// </summary>
    public ref struct SpanCharStream
    {
        readonly Span<char> Span;
        int m_position;
        public int Length
        {
            readonly get => m_position;
            private set => m_position = value;
        }

        public readonly int Capacity => Span.Length;
        public readonly Span<char> AsSpan => Span.Slice(0, m_position);
        public readonly string AsString => new string(AsSpan);


        public SpanCharStream(Span<char> span)
            : this()
        {
            Span = span;
        }

        public void Clear()
        {
            m_position = 0;
        }

        public void WriteChar(char c)
        {
            Span[m_position++] = c;
        }

        public void WriteString(ReadOnlySpan<char> str)
        {
            str.CopyTo(Span.Slice(m_position, str.Length));
            m_position += str.Length;
        }

        public void WriteInt(int value)
        {
            if (!value.TryFormat(Span.Slice(m_position), out int charsWritten))
                throw new ArgumentException($"Failed to format value: {value}");

            m_position += charsWritten;
        }

        public void WriteByte(byte value, ReadOnlySpan<char> format = default)
        {
            if (!value.TryFormat(Span.Slice(m_position), out int charsWritten, format))
                throw new ArgumentException($"Failed to format value: {value}");

            m_position += charsWritten;
        }

    }
}
