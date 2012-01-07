using System;
using System.IO;

namespace Journaler.PerfTests
{
    class Program
    {
        const int SectorSize = 512;
        private const int TestDuration = 30;

        static void Main()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "bench.dat");
            
            if(File.Exists(path)) File.Delete(path);

            Console.WriteLine("Test\t\tBuffer\tIOs/sec\tbytes/sec");

            var bufferSize = SectorSize;
            for (int i = 0; i < 10; i++)
            {
                var totalIosJournaler =  RunTestJournalWriter(path, bufferSize);
                var totalIosBlockWriter = RunTestBlockWriter(path, bufferSize);
                Console.WriteLine("{0:P}", (double)totalIosJournaler / totalIosBlockWriter);
                bufferSize *= 2;
            }

            Console.ReadKey();
        }

        private static long RunTestJournalWriter(string path, int bufferSize)
        {
            if(File.Exists(path))
            {
                File.Delete(path);
            }

            long ioCount = 0;
            var blockWriter = new FileBlockWriter(path, bufferSize);
            using (var journalWriter = new JournalWriter(bufferSize, blockWriter))
            {
                var buffer = new ArraySegment<byte>(new byte[bufferSize]);
                var start = DateTime.UtcNow;

                while (DateTime.UtcNow - start < TimeSpan.FromSeconds(TestDuration)) // run the test for 10sec
                {
                    journalWriter.WriteBytes(buffer, true);
                    ioCount++;
                }

                var durationInMs = (DateTime.UtcNow - start).TotalMilliseconds;
                PrintReport("JournalWriter", bufferSize, ioCount, durationInMs);
            }
            return ioCount;
        }

        private static long RunTestBlockWriter(string path, int bufferSize)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            long ioCount = 0;
            using (var blockWriter = new FileBlockWriter(path, bufferSize))
            {
                var buffer = new byte[bufferSize];
                var start = DateTime.UtcNow;

                while (DateTime.UtcNow - start < TimeSpan.FromSeconds(TestDuration)) // run the test for 10sec
                {
                    blockWriter.Write(buffer, true);
                    ioCount++;
                }

                var durationInMs = (DateTime.UtcNow - start).TotalMilliseconds;
                PrintReport("FileBlockWriter", bufferSize, ioCount, durationInMs);
            }

            return ioCount;
        }

        private static void PrintReport(string test, int bufferSize, long ioCount, double durationInMs)
        {
            var bytesPerSecond = ioCount * bufferSize * 1000L / durationInMs;
            var ioPerSec = ioCount * 1000L / durationInMs;

            Console.WriteLine("{0}\t{1}\t{2:n0}\t{3:n0}", test, bufferSize, ioPerSec, bytesPerSecond);
        }
    }
}
