using System;
using System.IO;

namespace UGameCore.Utilities
{
    public static class StreamExtensions
    {
        public static long SeekDefault(this Stream stream, long offset, SeekOrigin origin)
        {
            long newPosition = stream.Position;

            if (origin == SeekOrigin.Begin)
                newPosition = offset;
            else if (origin == SeekOrigin.Current)
                newPosition += offset;
            else if (origin == SeekOrigin.End)
                newPosition = stream.Length + offset;

            stream.Position = newPosition;

            return stream.Position;
        }

        public static void ValidateReadArguments(this Stream stream, byte[] outBuffer, int outBufferOffset, ref int countToRead)
        {
            long streamPosition = stream.Position;

            if (streamPosition < 0 || countToRead < 0 || outBufferOffset < 0)
                throw new ArgumentException("Less than 0");

            if (outBufferOffset + countToRead > outBuffer.Length)
                throw new ArgumentException("Invalid offset or count");

            if (countToRead == 0)
                return;

            long streamLength = stream.Length;

            if (streamPosition == streamLength) // valid situation
            {
                countToRead = 0;
                return;
            }

            if (streamPosition > streamLength)
                throw new ArgumentException($"Stream position {streamPosition} points outside of stream, length {streamLength}");

            if (streamPosition + countToRead > streamLength)
                countToRead = (int)(streamLength - streamPosition);
        }
    }
}
