﻿using System;

namespace Journaler
{
    /// <summary>
    /// Write blocks to disk
    /// </summary>
    public interface IBlockWriter : IDisposable
    {
        /// <summary>
        /// Write a block to disk and move to next block if specified
        /// </summary>
        /// <param name="block">The block to write to disk</param>
        /// <param name="moveToNextBlock">True to move to the next block, False otherwise.</param>
        void Write(byte[] block, bool moveToNextBlock);

        /// <summary>
        /// Pre-allocates the data container with the given size.
        /// </summary>
        /// <param name="blocksCount">Size of data container in number of blocks (size of the file is blockCount * blockSize)</param>
        void SetSize(int blocksCount);
    }
}