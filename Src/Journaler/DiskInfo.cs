using System.IO;
using System.Runtime.InteropServices;

namespace Journaler
{
    /// <summary>
    /// Class to retrieve the sector size of a volume that contains a given file.
    /// </summary>
    public static class DiskInfo
    {
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
        public static uint GetDriveSectorSize(string path)
        {
            uint size;       // sector size in bytes. 
            uint toss;       // ignored outputs
            GetDiskFreeSpace(Path.GetPathRoot(path), out toss, out size, out toss, out toss);
            return size;
        }
    }
}
