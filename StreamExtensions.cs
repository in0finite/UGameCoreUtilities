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

        public static void ReadBytesExactCount(this Stream stream, byte[] buffer, int count)
        {
            if (count > buffer.Length)
                throw new ArgumentException($"Trying to read {count} bytes into a buffer of size {buffer.Length}");

            ReadBytesExactCountArg(
                static (arg1, totalNumRead, countToRead) => arg1.stream.Read(arg1.buffer, totalNumRead, countToRead),
                (stream, buffer),
                count);
        }

        public static byte[] ReadBytesExactCount(this Stream stream, int count)
        {
            byte[] buffer = new byte[count];
            ReadBytesExactCount(stream, buffer, count);
            return buffer;
        }

        public static void ReadBytesExactCountArg<T>(Func<T, int, int, int> readFunc, T arg, int count)
        {
            if (count < 0)
                throw new ArgumentException("Count less than 0");

            if (count == 0)
                return;

            int totalNumRead = 0;

            while (true)
            {
                int numRead = readFunc(arg, totalNumRead, count - totalNumRead);
                if (numRead <= 0)
                    break;

                totalNumRead += numRead;

                if (totalNumRead >= count)
                    break;
            }

            if (totalNumRead != count)
                throw new IOException($"Failed to read exact number of bytes ({count}), read {totalNumRead} bytes");
        }

        public static void ReadExactCountGeneric<T>(Func<T[], int, int, int> readFunc, T[] buffer, int count)
        {
            ReadBytesExactCountArg(readFunc, buffer, count);
        }

        public static byte[] ReadAllUntilEnd(this Stream stream)
        {
            return stream.ReadBytesExactCount((int)(stream.Length - stream.Position));
        }

        public static bool TryGetMemoryStreamBuffer(this Stream stream, out ArraySegment<byte> arraySegment)
        {
            if (stream is MemoryStream memoryStream && memoryStream.TryGetBuffer(out arraySegment))
            {
                return true;
            }

            arraySegment = default;
            return false;
        }

        public static void EnsureCapacity(this MemoryStream memoryStream, int capacity)
        {
            if (memoryStream.Capacity < capacity)
                memoryStream.Capacity = capacity;
        }
    }
}
