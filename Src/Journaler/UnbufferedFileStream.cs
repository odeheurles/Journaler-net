using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Journaler
{
    /// <summary>
    /// Wrap win32 API used to open an unbuffered <see cref="FileStream"/>
    /// </summary>
    public static class UnbufferedFileStream
    {
        const int FileFlagNoBuffering = unchecked(0x20000000);
        const int FileFlagOverlapped = unchecked(0x40000000);
        const int FileFlagSequentialScan = unchecked(0x08000000);

        [DllImport("KERNEL32", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        static extern SafeFileHandle CreateFile(String fileName,
                                                   int desiredAccess,
                                                   FileShare shareMode,
                                                   IntPtr securityAttrs,
                                                   FileMode creationDisposition,
                                                   int flagsAndAttributes,
                                                   IntPtr templateFile);

        /// <summary>
        /// Opens a file in unbuffered mode (i.e. NTFS is told not to cache the file contents).
        /// </summary>
        /// <param name="path">File name</param>
        /// <param name="mode"> System.IO.FileMode </param>
        /// <param name="acc">System.IO.FileAccess: Read | Write | ReadWrite</param>
        /// <param name="share">System.IO.FileShare</param>
        /// <param name="sequential">sequential file access</param>
        /// <param name="useAsync">async file access</param>
        /// <param name="blockSize">block size in bytes</param>
        /// <returns>Unbuffered file stream.</returns>
        public static FileStream OpenStream(string path,
                                     FileMode mode,
                                     FileAccess acc,
                                     FileShare share,
                                     bool sequential,
                                     bool useAsync,
                                     int blockSize)
        {
            int flags = FileFlagNoBuffering;  
            if (sequential) flags |= FileFlagSequentialScan;
            if (useAsync) flags |= FileFlagOverlapped;

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
    }
}

