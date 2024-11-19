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

        public static SpanCharStream Empty => new SpanCharStream(Span<char>.Empty);


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

        public void WriteStringReplaced(
            ReadOnlySpan<char> str, ReadOnlySpan<char> stringToReplace, ReadOnlySpan<char> newString, StringComparison stringComparison)
        {
            int positionBefore = Length;
            WriteString(str);
            Replace(stringToReplace, newString, positionBefore, Length - positionBefore, stringComparison);
        }

        public void Replace(
            ReadOnlySpan<char> stringToReplace, ReadOnlySpan<char> newString, int replaceIndex, int replaceCount, StringComparison stringComparison)
        {
            CheckArguments(replaceIndex, replaceCount);

            if (stringToReplace.Length == 0)
                throw new ArgumentException("Empty string");

            if (replaceCount < stringToReplace.Length) // nothing to replace
                return;

            bool bInitializedTempBuffer = false;
            using PooledArray<byte> pooledArray = PooledArray<byte>.Empty();
            SpanCharStream tempSpanCharStream = SpanCharStream.Empty;

            int i = replaceIndex;
            int replaceEnd = replaceIndex + replaceCount;
            bool hasDoneAnyReplacements = false;
            int lastIndexOriginalBuffer = 0;

            while (i < replaceEnd)
            {
                ReadOnlySpan<char> searchSpan = Span.Slice(i, replaceEnd - i);

                int indexOf = searchSpan.IndexOf(stringToReplace, stringComparison);
                if (indexOf < 0)
                    break;

                indexOf += i; // re-adjust found index, because we sliced above

                hasDoneAnyReplacements = true;

                // initialize temp buffer
                if (!bInitializedTempBuffer)
                {
                    bInitializedTempBuffer = true;
                    pooledArray.Resize(Capacity * 2);
                    tempSpanCharStream = new SpanCharStream(pooledArray.Span.CastEntirely<byte, char>());
                }

                // copy from existing buffer up to current index, into temp buffer

                int numToCopyFromOriginalBuffer = indexOf - lastIndexOriginalBuffer;
                tempSpanCharStream.WriteString(Span.Slice(lastIndexOriginalBuffer, numToCopyFromOriginalBuffer));

                lastIndexOriginalBuffer += numToCopyFromOriginalBuffer + stringToReplace.Length;

                // append replaced string
                tempSpanCharStream.WriteString(newString);

                i = indexOf + stringToReplace.Length;
            }

            if (!hasDoneAnyReplacements)
                return;

            // append rest of original buffer

            tempSpanCharStream.WriteString(Span.Slice(lastIndexOriginalBuffer, Length - lastIndexOriginalBuffer));

            // copy from temp buffer into original buffer

            tempSpanCharStream.AsSpan.CopyTo(Span.Slice(0, tempSpanCharStream.Length));
            Length = tempSpanCharStream.Length;
        }

        readonly void CheckArguments(int index, int count)
        {
            if (index < 0 || count < 0)
                throw new ArgumentOutOfRangeException();

            if (index + count > Length)
                throw new ArgumentOutOfRangeException(nameof(index), $"{index} + {count} > {Length}");
        }

        readonly void CheckEnoughCapacity(int newLength)
        {
            if (newLength > Capacity)
                throw new InvalidOperationException($"Not enough capacity: {newLength} / {Capacity}");
        }

        public void Replace(
            int index, int count, ReadOnlySpan<char> newString)
        {
            CheckArguments(index, count);

            int newLength = Length - count + newString.Length;
            CheckEnoughCapacity(newLength);

            Span<char> spanToRemove = Span.Slice(index, count);

            if (newString.Length >= count)
            {
                // copy what's possible
                newString.Slice(0, count).CopyTo(spanToRemove);

                // insert the rest
                Insert(index + count, newString.Slice(count));
            }
            else // newString.Length < count
            {
                // copy everything
                newString.CopyTo(spanToRemove);

                // remove the rest
                Remove(index + newString.Length, count - newString.Length);
            }
        }

        public void Remove(int index, int count)
        {
            CheckArguments(index, count);

            int transferIndex = index + count;
            Span<char> transferSpan = Span.Slice(transferIndex, Length - transferIndex);

            if (count == 0)
                return;

            using PooledArray<byte> pooledArray = new PooledArray<byte>(transferSpan.Length * 2);
            Span<char> tempSpan = pooledArray.Span.CastEntirely<byte, char>();

            transferSpan.CopyTo(tempSpan);

            tempSpan.CopyTo(Span.Slice(index));

            m_position -= count;
        }

        public void Insert(int index, ReadOnlySpan<char> newString)
        {
            CheckEnoughCapacity(Length + newString.Length);

            Span<char> modifiedSpan = Span.Slice(index, Length - index);

            // make a backup of modified part of Span

            const int kMaxStackAllocCharsCount = 128; // 128 chars = 256 bytes
            bool bUsePooledArray = modifiedSpan.Length > kMaxStackAllocCharsCount;

            using PooledArray<byte> pooledArray = bUsePooledArray
                ? new PooledArray<byte>(modifiedSpan.Length * 2)
                : default;

            Span<char> tempSpan = bUsePooledArray
                ? pooledArray.Span.CastEntirely<byte, char>()
                : stackalloc char[modifiedSpan.Length];

            modifiedSpan.CopyTo(tempSpan);

            // insert
            newString.CopyTo(Span.Slice(index));

            // append backup
            tempSpan.CopyTo(Span.Slice(index + newString.Length));

            m_position += newString.Length;
        }
    }
}
