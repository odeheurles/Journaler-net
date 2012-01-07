using System;

namespace Journaler
{
    /// <summary>
    /// Write blocks to disk
    /// </summary>
    public interface IBlockWriter
    {
        /// <summary>
        /// Write a block to disk and move to next block if specified
        /// </summary>
        /// <param name="block">The block to write to disk</param>
        /// <param name="moveToNextBlock">True to move to the next block, False otherwise.</param>
        void Write(ArraySegment<byte> block, bool moveToNextBlock);
    }
}