using System;
using System.IO;
using NUnit.Framework;

namespace Journaler.Tests
{
    [TestFixture]
    public class FileBlockWriterTests
    {
        private string _path;
        private IBlockWriter _blockWriter;
        private const int BlockSize = 4*1024;

        [SetUp]
        public void SetUp()
        {
            _path = Path.Combine(Environment.CurrentDirectory, "FileBlockWriterTests.dat");
            if(File.Exists(_path))
            {
                File.Delete(_path);
            }
            _blockWriter = new FileBlockWriter(_path, BlockSize);
        }

        [TearDown]
        public void TearDown()
        {
            _blockWriter.Dispose();
            if(File.Exists(_path))
            {
                File.Delete(_path);
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorWithNullPathThrows()
        {
            new FileBlockWriter(null, 4 * 1024);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorWithEmptyPathThrows()
        {
            new FileBlockWriter("", 4 * 1024);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConstructorWithBlockSizeOfZeroThrows()
        {
            new FileBlockWriter("foo", 0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorWithBlockSizeNonMultipleOfSectorSizeThrows()
        {
            new FileBlockWriter(@"c:\", 1234);
        }

        [Test]
        public void ConstructorShouldCreateFile()
        {
            Assert.IsTrue(File.Exists(_path));
        }

        [Test]
        public void SetSizeShouldPreAllocateFile()
        {
            const int expectedBlocks = 32;
            _blockWriter.SetSize(expectedBlocks);
            _blockWriter.Dispose(); // release exclusive access file handle
            using(var stream = File.Open(_path, FileMode.Open))
            {
                Assert.AreEqual(expectedBlocks * BlockSize, stream.Length);                
            }
        }

        [Test]
        public void WriteShouldFlushToDisk()
        {
            const byte expected = (byte) 1;
            var data = GenerateArrayWithSameValues(expected, BlockSize);

            _blockWriter.Write(data, false);
            _blockWriter.Dispose(); // release exclusive access file handle

            using (var stream = File.Open(_path, FileMode.Open))
            {
                for (int i = 0; i < BlockSize; i++)
                {
                    Assert.AreEqual(expected, stream.ReadByte());
                }
            }
        }

        [Test]
        public void WriteToSameBlockTwoTimesShouldOverwrite()
        {
            _blockWriter.Write(GenerateArrayWithSameValues(1, BlockSize), false); // first write with ones

            const byte expected = 2;
            _blockWriter.Write(GenerateArrayWithSameValues(expected, BlockSize), false); // second write with twos

            _blockWriter.Dispose(); // release exclusive access file handle

            using (var stream = File.Open(_path, FileMode.Open))
            {
                for (int i = 0; i < BlockSize; i++)
                {
                    Assert.AreEqual(expected, stream.ReadByte());
                }
            }
        }

        [Test]
        public void WriteMoveWriteShouldWriteTwoBlocks()
        {
            const byte expected1 = (byte)1;
            const byte expected2 = (byte)2;

            _blockWriter.Write(GenerateArrayWithSameValues(expected1, BlockSize), true); // first write with 1s, and move
            _blockWriter.Write(GenerateArrayWithSameValues(expected2, BlockSize), false); // second write with 2s

            _blockWriter.Dispose(); // release exclusive access file handle

            using (var stream = File.Open(_path, FileMode.Open))
            {
                for (int i = 0; i < BlockSize; i++)
                {
                    Assert.AreEqual(expected1, stream.ReadByte(), "First block should contain 1s");
                }
                for (int i = 0; i < BlockSize; i++)
                {
                    Assert.AreEqual(expected2, stream.ReadByte(), "Second block should contain 2s");
                }
            }
        }

        private static byte[] GenerateArrayWithSameValues(byte value, int count)
        {
            var data = new byte[count];
            for (int i = 0; i < count; i++)
            {
                data[i] = value;
            }
            return data;
        }
    }
}
