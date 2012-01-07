using System;
using NUnit.Framework;

namespace Journaler.Tests
{
    [TestFixture]
    public class ByteRingBufferTests
    {
        private ByteRingBuffer _buffer;
        private const int BufferSize = 10;

        [SetUp]
        public void SetUp()
        {
            _buffer = new ByteRingBuffer(BufferSize);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConstructorWithNegativeSizeThrows()
        {
            new ByteRingBuffer(-1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConstructorWithSizeOfZeroThrows()
        {
            new ByteRingBuffer(0);
        }

        [Test] 
        public void SizeEqualsInitialSize()
        {
            Assert.AreEqual(BufferSize, _buffer.Size);
        }

        [Test]
        public void DefaultPositionIsZero()
        {
            Assert.AreEqual(0, _buffer.Position);
        }

        [Test]
        public void SetPositionWithValidValue()
        {
            const int expectedPosition = 4;

            _buffer.Position = expectedPosition;

            Assert.AreEqual(expectedPosition, _buffer.Position);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetNegativePositionThrows()
        {
            _buffer.Position = -1;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetPositionGreaterThanSizeThrows()
        {
            _buffer.Position = BufferSize;
        }

        [Test]
        public void WriteByteIncreasePositionByOne()
        {
            _buffer.WriteByte(1);
            Assert.AreEqual(sizeof(byte), _buffer.Position);
        }

        [Test]
        public void WriteByteThatDoesNotWrapReturnsFalse()
        {
            Assert.IsFalse(_buffer.WriteByte(1));
        }

        [Test]
        public void WriteByteThatWrapReturnsTrue()
        {
            _buffer.Position = BufferSize - 1;
            Assert.IsTrue(_buffer.WriteByte(1));
        }

        [Test]
        public void WriteByteProperlyUpdateBuffer()
        {
            const byte expected = default(byte);
            _buffer.WriteByte(expected);

            Assert.AreEqual(expected, _buffer[0]);
        }

        [Test]
        public void WriteByteRaisesBufferFullEventIfBufferWraps()
        {
            const byte value = 1;
            _buffer.Position = BufferSize - 1;
            int eventRaisedCount = 0;

            _buffer.BufferFull +=
                () =>
                    {
                        eventRaisedCount++;
                    };
            _buffer.WriteByte(value);

            Assert.AreEqual(0, _buffer.Position);
            Assert.AreEqual(1, eventRaisedCount);
        }

        [Test]
        public void WriteLongIncreasePositionBySizeOfLong()
        {
            _buffer.WriteLong(1L);
            Assert.AreEqual(sizeof(long), _buffer.Position);
        }

        [Test]
        public void WriteLongThatDoesNotWrapReturnsFalse()
        {
            Assert.IsFalse(_buffer.WriteLong(1L));
        }

        [Test]
        public void WriteLongThatWrapReturnsTrue()
        {
            _buffer.Position = BufferSize - 1;
            Assert.IsTrue(_buffer.WriteLong(1L));
        }

        [Test]
        public void WriteLongProperlyUpdateBuffer()
        {
            const long input = 123456789L;
            var expected = BitConverter.GetBytes(input);

            _buffer.WriteLong(input);
            for (int i = 0; i < sizeof(long); i++)
            {
                Assert.AreEqual(expected[i], _buffer[i]);
            }
        }

        [Test]
        public void WriteLongShouldRaiseFullEventWhenBufferWrap()
        {
            int eventRaisedCount = 0;
            _buffer.BufferFull +=
                () =>
                {
                    eventRaisedCount++;
                };
            _buffer.Position = BufferSize - 4;

            _buffer.WriteLong(1L);

            Assert.AreEqual(4, _buffer.Position);
            Assert.AreEqual(1, eventRaisedCount);
        }

        [Test]
        public void WriteIntIncreasePositionBySizeOfLong()
        {
            _buffer.WriteInt(1);
            Assert.AreEqual(sizeof(int), _buffer.Position);
        }

        [Test]
        public void WriteIntThatDoesNotWrapReturnsFalse()
        {
            Assert.IsFalse(_buffer.WriteInt(1));
        }

        [Test]
        public void WriteIntThatWrapReturnsTrue()
        {
            _buffer.Position = BufferSize - 1;
            Assert.IsTrue(_buffer.WriteInt(1));
        }

        [Test]
        public void WriteIntProperlyUpdateBuffer()
        {
            const int input = 12345;
            var expected = BitConverter.GetBytes(input);

            _buffer.WriteInt(input);
            for (int i = 0; i < sizeof(int); i++)
            {
                Assert.AreEqual(expected[i], _buffer[i]);
            }
        }

        [Test]
        public void WriteIntShouldRaiseFullEventWhenBufferWrap()
        {
            int eventRaisedCount = 0;
            _buffer.BufferFull +=
                () =>
                {
                    eventRaisedCount++;
                };
            _buffer.Position = BufferSize - 2;

            _buffer.WriteInt(1);

            Assert.AreEqual(2, _buffer.Position);
            Assert.AreEqual(1, eventRaisedCount);
        }

        [Test]
        public void WriteBytesShouldUpdatePosition()
        {
            const int expectedPosition = 5;
            var input = new byte[expectedPosition];

            _buffer.WriteBytes(new ArraySegment<byte>(input));
            Assert.AreEqual(expectedPosition, _buffer.Position);
        }

        [Test]
        public void WriteBytesThatDoesNotWrapReturnsFalse()
        {
            Assert.IsFalse(_buffer.WriteBytes(new ArraySegment<byte>(new byte[1])));
        }

        [Test]
        public void WriteBytesThatWrapReturnsTrue()
        {
            _buffer.Position = BufferSize - 1;
            Assert.IsTrue(_buffer.WriteBytes(new ArraySegment<byte>(new byte[2])));
        }

        [Test]
        public void WriteBytesProperlyUpdatesBuffer()
        {
            byte[] bytes = {1, 2, 3, 4};

            _buffer.WriteBytes(new ArraySegment<byte>(bytes));
            for (int i = 0; i < bytes.Length; i++)
            {
                Assert.AreEqual(bytes[i], _buffer[i]);
            }
        }

        [Test]
        public void WriteBytesRaisesBufferFullEventWhenWraps()
        {
            int eventRaisedCount = 0;
            _buffer.BufferFull +=
                () =>
                {
                    eventRaisedCount++;
                };

            var bytes = new byte[35];

            _buffer.WriteBytes(new ArraySegment<byte>(bytes));
            Assert.AreEqual(3, eventRaisedCount);
        }
    }
}