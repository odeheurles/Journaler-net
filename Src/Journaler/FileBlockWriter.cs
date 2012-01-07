using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Journaler
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class FileBlockWriter : IBlockWriter
    {
        private readonly int _blockSizeInBytes;
        private FileStream _stream;

        /// <summary>
        /// Create a new <see cref="FileBlockWriter"/>
        /// </summary>
        /// <param name="path">full path of the new file.</param>
        /// <param name="blockSizeInBytes">size of the blocks in bytes</param>
        /// <exception cref="ArgumentOutOfRangeException">If blockSizeInBytes is smaller or equal to 0.</exception>
        /// <exception cref="ArgumentException">if path is null or empty or if blockSize is not a multiple of the disk sector size.</exception>
        public FileBlockWriter(string path, int blockSizeInBytes)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("path was null or empty", "path");
            if (blockSizeInBytes <= 0) throw new ArgumentOutOfRangeException("blockSizeInBytes");

            var sectorSizeInBytes = GetDriveSectorSize(path);
            if (blockSizeInBytes % sectorSizeInBytes != 0) 
                throw new ArgumentException(string.Format("block size must be a multiple of sector size ({0}bytes).", sectorSizeInBytes));

            _blockSizeInBytes = blockSizeInBytes;
            _stream = OpenStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, false, false, blockSizeInBytes);
        }

        /// <summary>
        /// Pre-allocates the file on disk with the given size.
        /// This improves speed compared to a fragmented file. 
        /// </summary>
        /// <param name="blocksCount">Size of the files in number of blocks (size of the file is blockCount * blockSize)</param>
        public void SetSize(int blocksCount)
        {
            _stream.SetLength(blocksCount * _blockSizeInBytes);
            _stream.Seek(0, SeekOrigin.Begin);
        }

        /// <summary>
        /// Write a block to disk and move to next block if specified
        /// </summary>
        /// <param name="block">The block to write to disk</param>
        /// <param name="moveToNextBlock">True to move to the next block, False otherwise.</param>
        public void Write(byte[] block, bool moveToNextBlock)
        {
            _stream.Write(block, 0, block.Length);

            if(!moveToNextBlock)
            {
                // seek back
                _stream.Seek(-_blockSizeInBytes, SeekOrigin.Current);
            }
        }

        /// <summary>
        /// Dispose the instance
        /// </summary>
        public void Dispose()
        {
            if(_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }
        }

        /// <summary>
        /// The file or device is being opened with no system caching for data reads and writes. This flag does not affect hard disk caching or memory mapped files.
        /// There are strict requirements for successfully working with files opened with CreateFile using the FILE_FLAG_NO_BUFFERING flag, for details see File Buffering.
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/cc644950(v=vs.85).aspx
        /// </summary>
        // ReSharper disable InconsistentNaming
        private const int FILE_FLAG_NO_BUFFERING = unchecked(0x20000000);

        /// <summary>
        /// The file or device is being opened or created for asynchronous I/O.
        /// When subsequent I/O operations are completed on this handle, the event specified in the OVERLAPPED structure will be set to the signaled state.
        /// If this flag is specified, the file can be used for simultaneous read and write operations.
        /// If this flag is not specified, then I/O operations are serialized, even if the calls to the read and write functions specify an OVERLAPPED structure.
        /// </summary>
        private const int FILE_FLAG_OVERLAPPED = unchecked(0x40000000);

        /// <summary>
        /// Access is intended to be sequential from beginning to end. The system can use this as a hint to optimize file caching.
        /// This flag should not be used if read-behind (that is, reverse scans) will be used.
        /// This flag has no effect if the file system does not support cached I/O and FILE_FLAG_NO_BUFFERING.
        /// </summary>
        private const int FILE_FLAG_SEQUENTIAL_SCAN = unchecked(0x08000000);

        /// <summary>
        /// Write operations will not go through any intermediate cache, they will go directly to disk.
        /// For additional information, see the Caching Behavior section of this topic.
        /// </summary>
        //private const int FILE_FLAG_WRITE_THROUGH = unchecked(0x80000000);
        // ReSharper restore InconsistentNaming

        [DllImport("KERNEL32", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        static extern SafeFileHandle CreateFile(String fileName,
                                                   int desiredAccess,
                                                   FileShare shareMode,
                                                   IntPtr securityAttrs,
                                                   FileMode creationDisposition,
                                                   int flagsAndAttributes,
                                                   IntPtr templateFile);

        private static FileStream OpenStream(string path,
                                     FileMode mode,
                                     FileAccess acc,
                                     FileShare share,
                                     bool sequential,
                                     bool useAsync,
                                     int blockSize)
        {
            int flags = FILE_FLAG_NO_BUFFERING;
            if (sequential) flags |= FILE_FLAG_SEQUENTIAL_SCAN;
            if (useAsync) flags |= FILE_FLAG_OVERLAPPED;

            var handle = CreateFile(path,
                                 (int)acc,
                                 share,
                                 IntPtr.Zero,
                                 mode,
                                 flags,
                                 IntPtr.Zero);

            if (!handle.IsInvalid)
            {
                return new FileStream(handle, acc, blockSize, useAsync);
            }
            throw new IOException("Failed to create the file handle");
        }

        [DllImport("KERNEL32", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        static extern bool GetDiskFreeSpace(string path,
                                            out uint sectorsPerCluster,
                                            out uint bytesPerSector,
                                            out uint numberOfFreeClusters,
                                            out uint totalNumberOfClusters);
        /// <summary>
        /// Return the sector size of the volume the specified filepath lives on.
        /// </summary>
        /// <param name="path">UNC path name for the file or directory</param>
        /// <returns>device sector size in bytes </returns>
        private static uint GetDriveSectorSize(string path)
        {
            uint size; // sector size in bytes. 
            uint toss; // ignored outputs
            GetDiskFreeSpace(Path.GetPathRoot(path), out toss, out size, out toss, out toss);
            return size;
        }
    }
}