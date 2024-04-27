using System;
using System.IO;

namespace UGameCore.Utilities
{
    public class BufferedReadStream : Stream
    {
        Stream m_stream;

        byte[] m_buffer = Array.Empty<byte>();
        long m_bufferPosInStream = 0;
        int m_bufferUsedLength = 0;

        readonly int m_maxBufferSize = 0;

        byte[] m_singleSizeArray = new byte[1];


        public BufferedReadStream(Stream stream, int maxBufferSize)
        {
            if (!stream.CanRead)
                throw new InvalidOperationException("Stream not readable");

            if (!stream.CanSeek)
                throw new InvalidOperationException("Stream not seekable");

            if (maxBufferSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxBufferSize));

            m_stream = stream;
            m_maxBufferSize = Math.Min(maxBufferSize, (int)stream.Length); // no need to allocate buffer larger than stream length
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => m_stream.Length;

        public override long Position { get => m_stream.Position; set => m_stream.Position = value; }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] outBuffer, int outBufferOffset, int countToRead)
        {
            m_stream.ValidateReadArguments(outBuffer, outBufferOffset, ref countToRead);

            if (countToRead == 0)
                return 0;

            if (IsInsideBuffer(countToRead, out int offsetInCachedBuffer, out int lengthInCachedBuffer))
            {
                // copy from cached buffer
                Buffer.BlockCopy(m_buffer, offsetInCachedBuffer, outBuffer, outBufferOffset, lengthInCachedBuffer);

                m_stream.Position += lengthInCachedBuffer;
                outBufferOffset += lengthInCachedBuffer;
                
                // read remaining data

                int countRemainingToRead = countToRead - lengthInCachedBuffer;
                if (countRemainingToRead <= 0) // no remaining data left
                    return lengthInCachedBuffer;

                // we can simply call the function recursively
                return this.Read(outBuffer, outBufferOffset, countRemainingToRead) + lengthInCachedBuffer;
            }
            else
            {
                // data is not in cached buffer

                // if data to read is too large, forward call to underlying stream

                if (countToRead > m_maxBufferSize)
                    return m_stream.Read(outBuffer, outBufferOffset, countToRead);

                // discard cached buffer, and fetch new data into it

                m_bufferUsedLength = 0;
                m_bufferPosInStream = long.MinValue; // set to invalid value

                if (m_buffer.Length == 0)
                    m_buffer = new byte[m_maxBufferSize];

                long oldPosition = m_stream.Position;

                m_bufferUsedLength = m_stream.Read(m_buffer, 0, m_buffer.Length);

                m_bufferPosInStream = oldPosition;
                m_stream.Position = oldPosition;

                // copy data from cached buffer to output buffer

                // we can simply call the function recursively
                return this.Read(outBuffer, outBufferOffset, Math.Min(countToRead, m_bufferUsedLength));
            }
        }

        bool IsInsideBuffer(int countToRead, out int offset, out int length)
        {
            offset = 0;
            length = 0;

            if (m_bufferUsedLength == 0)
                return false;

            long currentPosition = m_stream.Position;
            long readingEndPosition = currentPosition + countToRead;

            long bufferEndPosInStream = m_bufferPosInStream + m_bufferUsedLength;

            if (currentPosition >= m_bufferPosInStream && currentPosition < bufferEndPosInStream)
            {
                offset = (int)(currentPosition - m_bufferPosInStream);
                length = (int)(Math.Min(bufferEndPosInStream, readingEndPosition) - currentPosition);

                return true;
            }

            return false;
        }

        // override to avoid memory allocation in Stream.ReadByte()
        public override int ReadByte()
        {
            int n = this.Read(m_singleSizeArray, 0, 1);
            if (n <= 0)
                return -1;
            return m_singleSizeArray[0];
        }

        public override long Seek(long offset, SeekOrigin origin) => m_stream.SeekDefault(offset, origin);

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            m_buffer = null;
            m_singleSizeArray = null;
            m_stream?.Dispose();
            m_stream = null;
            base.Dispose(disposing);
        }
    }
}
