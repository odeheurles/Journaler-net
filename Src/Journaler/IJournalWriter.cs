using System;

namespace Journaler
{
    /// <summary>
    /// TODO
    /// </summary>
    public interface IJournalWriter : IDisposable
    {
        /// <summary>
        /// Write a single byte to the journal
        /// </summary>
        /// <param name="value">the value to write to the journal</param>
        /// <param name="flush">True to flush to disk, false otherwise.</param>
        /// <returns>The same instance of <see cref="IJournalWriter"/>, to allow call chaining</returns>
        IJournalWriter WriteByte(byte value, bool flush);

        /// <summary>
        /// Write an int to the journal
        /// </summary>
        /// <param name="value">the value to write to the journal</param>
        /// <param name="flush">True to flush to disk, false otherwise.</param>
        /// <returns>The same instance of <see cref="IJournalWriter"/>, to allow call chaining</returns>
        IJournalWriter WriteInt(int value, bool flush);

        /// <summary>
        /// Write a long to the journal
        /// </summary>
        /// <param name="value">the value to write to the journal</param>
        /// <param name="flush">True to flush to disk, false otherwise.</param>
        /// <returns>The same instance of <see cref="IJournalWriter"/>, to allow call chaining</returns>
        IJournalWriter WriteLong(long value, bool flush);

        /// <summary>
        /// Write an byte array to the journal
        /// </summary>
        /// <param name="value">the value to write to the journal</param>
        /// <param name="flush">True to flush to disk, false otherwise.</param>
        /// <returns>The same instance of <see cref="IJournalWriter"/>, to allow call chaining</returns>
        IJournalWriter WriteBytes(ArraySegment<byte> value, bool flush);
    }
}