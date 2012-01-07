using System;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace Journaler.Tests
{
    [TestFixture]
    public class JournalWriterTests
    {
        private IJournalWriter _writer;
        private Mock<IBlockWriter> _blockWriterMock;
        private const int BufferSize = 32;

        [SetUp]
        public void SetUp()
        {
            _blockWriterMock = new Mock<IBlockWriter>();
            _writer = new JournalWriter(BufferSize, _blockWriterMock.Object);
        }

        [Test]
        public void WriteByteDoesNotCallBlockWriterAutomatically()
        {
            _writer.WriteByte(1, false);
            _blockWriterMock.Verify(blockWriter => blockWriter.Write(It.IsAny<byte[]>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void WriteByteWithFlushWritesToBlockWriter()
        {
            const byte value = 1;
            _blockWriterMock.Setup(blockWriter=>blockWriter.Write(It.IsAny<byte[]>(), It.IsAny<bool>()))
                            .Callback((byte[] block, bool moveToNextBlock) =>
                                          {
                                              Assert.AreEqual(false, moveToNextBlock);
                                              Assert.AreEqual(BufferSize, block.Length);
                                              Assert.AreEqual(value, block[0]);
                                          })
                            .Verifiable();

            _writer.WriteByte(value, true);

            _blockWriterMock.Verify(blockWriter => blockWriter.Write(It.IsAny<byte[]>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void WriteByteAtEndOfBlockWritesToBlockWriterOnce()
        {
            const byte value = 1;
            MoveToEndOfBlockMinus(sizeof(byte));

            _blockWriterMock.Setup(blockWriter => blockWriter.Write(It.IsAny<byte[]>(), It.IsAny<bool>()))
                            .Callback((byte[] block, bool moveToNextBlock) =>
                                            {
                                                Assert.AreEqual(true, moveToNextBlock);
                                                Assert.AreEqual(value, block[BufferSize - 1]);
                                            })
                            .Verifiable();

            _writer.WriteByte(value, true);

            _blockWriterMock.Verify(blockWriter => blockWriter.Write(It.IsAny<byte[]>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void WriteByteReturnsSameJournalWriterInstance()
        {
            Assert.AreSame(_writer, _writer.WriteByte(1, false));
        }

        [Test]
        public void WriteIntDoesNotCallBlockWriterAutomatically()
        {
            _writer.WriteInt(1, false);
            _blockWriterMock.Verify(blockWriter => blockWriter.Write(It.IsAny<byte[]>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void WriteIntWithFlushWritesToBlockWriter()
        {
            _blockWriterMock.Setup(blockWriter => blockWriter.Write(It.IsAny<byte[]>(), It.IsAny<bool>()))
                            .Callback((byte[] block, bool moveToNextBlock) =>
                            {
                                Assert.AreEqual(false, moveToNextBlock);
                                Assert.AreEqual(BufferSize, block.Length);
                                Assert.AreEqual(1, BitConverter.ToInt32(block, 0));
                            })
                            .Verifiable();

            _writer.WriteInt(1, true);

            _blockWriterMock.Verify(blockWriter => blockWriter.Write(It.IsAny<byte[]>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void WriteIntAtEndOfBlockWritesToBlockWriterOnce()
        {
            MoveToEndOfBlockMinus(sizeof(int));

            _blockWriterMock.Setup(blockWriter => blockWriter.Write(It.IsAny<byte[]>(), It.IsAny<bool>()))
                            .Callback((byte[] block, bool moveToNextBlock) => Assert.AreEqual(true, moveToNextBlock))
                            .Verifiable();

            _writer.WriteInt(1, true);

            _blockWriterMock.Verify(blockWriter => blockWriter.Write(It.IsAny<byte[]>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void WriteIntAcrossBlocksCallsBlockWriterTwice()
        {
            MoveToEndOfBlockMinus(1);
            bool firstCallback = true;

            // need sequence in Moq..
            _blockWriterMock.Setup(blockWriter => blockWriter.Write(It.IsAny<byte[]>(), It.IsAny<bool>()))
                            .Callback((byte[] block, bool moveToNextBlock) =>
                                          {
                                              if(firstCallback)
                                              {
                                                  Assert.AreEqual(true, moveToNextBlock);
                                                  firstCallback = false;
                                              }
                                              else
                                              {
                                                  Assert.AreEqual(false, moveToNextBlock);
                                              }
                                          })
                            .Verifiable();
            
            _writer.WriteInt(1, true);

            _blockWriterMock.Verify(blockWriter => blockWriter.Write(It.IsAny<byte[]>(), It.IsAny<bool>()), Times.Exactly(2));
        }

        [Test]
        public void WriteIntReturnsSameJournalWriterInstance()
        {
            Assert.AreSame(_writer, _writer.WriteInt(1, false));
        }

        [Test]
        public void WriteLongDoesNotCallBlockWriterAutomatically()
        {
            _writer.WriteLong(1, false);
            _blockWriterMock.Verify(blockWriter => blockWriter.Write(It.IsAny<byte[]>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void WriteLongWithFlushWritesToBlockWriter()
        {
            _blockWriterMock.Setup(blockWriter => blockWriter.Write(It.IsAny<byte[]>(), It.IsAny<bool>()))
                            .Callback((byte[] block, bool moveToNextBlock) =>
                            {
                                Assert.AreEqual(false, moveToNextBlock);
                                Assert.AreEqual(BufferSize, block.Length);
                                Assert.AreEqual(1, BitConverter.ToInt64(block, 0));
                            })
                            .Verifiable();

            _writer.WriteLong(1, true);

            _blockWriterMock.Verify(blockWriter => blockWriter.Write(It.IsAny<byte[]>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void WriteLongAtEndOfBlockWritesToBlockWriterOnce()
        {
            MoveToEndOfBlockMinus(sizeof(long));

            _blockWriterMock.Setup(blockWriter => blockWriter.Write(It.IsAny<byte[]>(), It.IsAny<bool>()))
                            .Callback((byte[] block, bool moveToNextBlock) => Assert.AreEqual(true, moveToNextBlock))
                            .Verifiable();

            _writer.WriteLong(1, true);

            _blockWriterMock.Verify(blockWriter => blockWriter.Write(It.IsAny<byte[]>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void WriteLongAcrossBlocksCallsBlockWriterTwice()
        {
            MoveToEndOfBlockMinus(1);
            bool firstCallback = true;

            // need sequence in Moq..
            _blockWriterMock.Setup(blockWriter => blockWriter.Write(It.IsAny<byte[]>(), It.IsAny<bool>()))
                            .Callback((byte[] block, bool moveToNextBlock) =>
                            {
                                if (firstCallback)
                                {
                                    Assert.AreEqual(true, moveToNextBlock);
                                    firstCallback = false;
                                }
                                else
                                {
                                    Assert.AreEqual(false, moveToNextBlock);
                                }
                            })
                            .Verifiable();

            _writer.WriteLong(1, true);

            _blockWriterMock.Verify(blockWriter => blockWriter.Write(It.IsAny<byte[]>(), It.IsAny<bool>()), Times.Exactly(2));
        }

        [Test]
        public void WriteLongReturnsSameJournalWriterInstance()
        {
            Assert.AreSame(_writer, _writer.WriteLong(1, false));
        }

        [Test]
        public void WriteBytesDoesNotCallBlockWriterAutomatically()
        {
            _writer.WriteBytes(CreateArraySegment(4), false);
            _blockWriterMock.Verify(blockWriter => blockWriter.Write(It.IsAny<byte[]>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void WriteBytesWithFlushWritesToBlockWriter()
        {
            const int length = 4;
            var input = CreateArraySegment(length);

            _blockWriterMock.Setup(blockWriter => blockWriter.Write(It.IsAny<byte[]>(), It.IsAny<bool>()))
                            .Callback((byte[] block, bool moveToNextBlock) =>
                            {
                                Assert.AreEqual(false, moveToNextBlock);
                                Assert.AreEqual(BufferSize, block.Length);
                                for (int i = 0; i < length; i++)
                                {
                                    Assert.AreEqual(input.Array[i], block[i]);
                                }
                            })
                            .Verifiable();

            _writer.WriteBytes(input, true);

            _blockWriterMock.Verify(blockWriter => blockWriter.Write(It.IsAny<byte[]>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void WriteBytesAtEndOfBlockWritesToBlockWriterOnce()
        {
            const int length = 4;
            MoveToEndOfBlockMinus(length);

            _blockWriterMock.Setup(blockWriter => blockWriter.Write(It.IsAny<byte[]>(), It.IsAny<bool>()))
                            .Callback((byte[] block, bool moveToNextBlock) => Assert.AreEqual(true, moveToNextBlock))
                            .Verifiable();

            _writer.WriteBytes(CreateArraySegment(length), true);

            _blockWriterMock.Verify(blockWriter => blockWriter.Write(It.IsAny<byte[]>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void WriteBytesAcrossBlocksCallsBlockWriterTwice()
        {
            MoveToEndOfBlockMinus(1);
            bool firstCallback = true;

            // need sequence in Moq..
            _blockWriterMock.Setup(blockWriter => blockWriter.Write(It.IsAny<byte[]>(), It.IsAny<bool>()))
                            .Callback((byte[] block, bool moveToNextBlock) =>
                            {
                                if (firstCallback)
                                {
                                    Assert.AreEqual(true, moveToNextBlock);
                                    firstCallback = false;
                                }
                                else
                                {
                                    Assert.AreEqual(false, moveToNextBlock);
                                }
                            })
                            .Verifiable();

            _writer.WriteBytes(CreateArraySegment(4), true);

            _blockWriterMock.Verify(blockWriter => blockWriter.Write(It.IsAny<byte[]>(), It.IsAny<bool>()), Times.Exactly(2));
        }

        [Test]
        public void WriteBytesReturnsSameJournalWriterInstance()
        {
            Assert.AreSame(_writer, _writer.WriteBytes(CreateArraySegment(1), false));
        }

        private void MoveToEndOfBlockMinus(int offset)
        {
            for (int i = 0; i < BufferSize - offset; i++)
            {
                _writer.WriteByte(0, false);
            }
        }

        private static ArraySegment<byte> CreateArraySegment(int length)
        {
            return new ArraySegment<byte>(Enumerable.Range(0, length).Select(i=>(byte)i).ToArray());
        }
    }
}
