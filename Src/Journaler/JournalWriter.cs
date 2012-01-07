using System;

namespace Journaler
{
    /// <summary>
    /// TODO
    /// </summary>
    public class JournalWriter : IJournalWriter
    {
        private readonly IBlockWriter _blockWriter;
        private readonly ByteRingBuffer _buffer;

        /// <summary>
        /// Create a new instance of <see cref="JournalWriter"/>
        /// </summary>
        /// <param name="bufferSize">size of the buffer, IOs will be writen to disk in blocks of this size. Must be a multiple of the disk sector size.</param>
        /// <param name="blockWriter">the <see cref="IBlockWriter"/> used to write to disk.</param>
        public JournalWriter(int bufferSize, IBlockWriter blockWriter)
        {
            _blockWriter = blockWriter;
            _buffer = new ByteRingBuffer(bufferSize);
            _buffer.BufferFull += OnBufferFull;
        }

        /// <summary>
        /// Write a single byte to the journal
        /// </summary>
        /// <param name="value">the value to write to the journal</param>
        /// <param name="flush">True to flush to disk, false otherwise.</param>
        /// <returns>The same instance of <see cref="IJournalWriter"/>, to allow call chaining</returns>
        public IJournalWriter WriteByte(byte value, bool flush)
        {
            _buffer.WriteByte(value);
            WriteBlockIfRequired(flush);
            return this;
        }

        /// <summary>
        /// Write an int to the journal
        /// </summary>
        /// <param name="value">the value to write to the journal</param>
        /// <param name="flush">True to flush to disk, false otherwise.</param>
        /// <returns>The same instance of <see cref="IJournalWriter"/>, to allow call chaining</returns>
        public IJournalWriter WriteInt(int value, bool flush)
        {
            _buffer.WriteInt(value);
            WriteBlockIfRequired(flush);
            return this;
        }

        /// <summary>
        /// Write a long to the journal
        /// </summary>
        /// <param name="value">the value to write to the journal</param>
        /// <param name="flush">True to flush to disk, false otherwise.</param>
        /// <returns>The same instance of <see cref="IJournalWriter"/>, to allow call chaining</returns>
        public IJournalWriter WriteLong(long value, bool flush)
        {
            _buffer.WriteLong(value);
            WriteBlockIfRequired(flush);
            return this;
        }

        /// <summary>
        /// Write an byte array to the journal
        /// </summary>
        /// <param name="value">the value to write to the journal</param>
        /// <param name="flush">True to flush to disk, false otherwise.</param>
        /// <returns>The same instance of <see cref="IJournalWriter"/>, to allow call chaining</returns>
        public IJournalWriter WriteBytes(ArraySegment<byte> value, bool flush)
        {
            _buffer.WriteBytes(value);
            WriteBlockIfRequired(flush);
            return this;
        }

        private void WriteBlock(bool moveToNextBlock)
        {
            _blockWriter.Write(_buffer.AsArray(), moveToNextBlock);
        }

        private void WriteBlockIfRequired(bool flush)
        {
            if (flush && _buffer.Position != 0)
            {
                WriteBlock(false);
            }
        }

        private void OnBufferFull()
        {
            WriteBlock(true);
        }
    }
}