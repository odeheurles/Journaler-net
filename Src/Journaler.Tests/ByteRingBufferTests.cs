using System;
using NUnit.Framework;

namespace Journaler.Tests
{
    [TestFixture]
    public class ByteRingBufferTests
    {
        private ByteRingBuffer _byteRingBuffer;
        private const int BufferSize = 10;

        [SetUp]
        public void SetUp()
        {
            _byteRingBuffer = new ByteRingBuffer(BufferSize);
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
            Assert.AreEqual(BufferSize, _byteRingBuffer.Size);
        }

        [Test]
        public void DefaultPositionIsZero()
        {
            Assert.AreEqual(0, _byteRingBuffer.Position);
        }

        [Test]
        public void SetPositionWithValidValue()
        {
            const int expectedPosition = 4;

            _byteRingBuffer.Position = expectedPosition;

            Assert.AreEqual(expectedPosition, _byteRingBuffer.Position);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetNegativePositionThrows()
        {
            _byteRingBuffer.Position = -1;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetPositionGreaterThanSizeThrows()
        {
            _byteRingBuffer.Position = BufferSize;
        }

        [Test]
        public void WriteByteIncreasePositionByOne()
        {
            _byteRingBuffer.WriteByte(1);
            Assert.AreEqual(sizeof(byte), _byteRingBuffer.Position);
        }

        [Test]
        public void WriteByteReturnsSameInstance()
        {
            Assert.AreSame(_byteRingBuffer, _byteRingBuffer.WriteByte(1));
        }

        [Test]
        public void WriteByteProperlyUpdateBuffer()
        {
            const byte expected = default(byte);
            _byteRingBuffer.WriteByte(expected);

            Assert.AreEqual(expected, _byteRingBuffer[0]);
        }

        [Test]
        public void WriteByteRaisesBufferFullEventIsBufferWraps()
        {
            const byte value = 1;
            _byteRingBuffer.Position = BufferSize - 1;
            int eventRaisedCount = 0;

            _byteRingBuffer.BufferFull +=
                () =>
                    {
                        eventRaisedCount++;
                    };
            _byteRingBuffer.WriteByte(value);

            Assert.AreEqual(0, _byteRingBuffer.Position);
            Assert.AreEqual(1, eventRaisedCount);
        }

        [Test]
        public void WriteLongIncreasePositionBySizeOfLong()
        {
            _byteRingBuffer.WriteLong(1L);
            Assert.AreEqual(sizeof(long), _byteRingBuffer.Position);
        }

        [Test]
        public void WriteLongReturnsSameInstance()
        {
            Assert.AreSame(_byteRingBuffer, _byteRingBuffer.WriteLong(1L));   
        }

        [Test]
        public void WriteLongProperlyUpdateBuffer()
        {
            const long input = 123456789L;
            var expected = BitConverter.GetBytes(input);

            _byteRingBuffer.WriteLong(input);
            for (int i = 0; i < sizeof(long); i++)
            {
                Assert.AreEqual(expected[i], _byteRingBuffer[i]);
            }
        }

        [Test]
        public void WriteLongShouldRaiseFullEventWhenBufferWrap()
        {
            int eventRaisedCount = 0;
            _byteRingBuffer.BufferFull +=
                () =>
                {
                    eventRaisedCount++;
                };
            _byteRingBuffer.Position = BufferSize - 4;

            _byteRingBuffer.WriteLong(1L);

            Assert.AreEqual(4, _byteRingBuffer.Position);
            Assert.AreEqual(1, eventRaisedCount);
        }

        [Test]
        public void WriteIntIncreasePositionBySizeOfLong()
        {
            _byteRingBuffer.WriteInt(1);
            Assert.AreEqual(sizeof(int), _byteRingBuffer.Position);
        }

        [Test]
        public void WriteIntReturnsSameInstance()
        {
            Assert.AreSame(_byteRingBuffer, _byteRingBuffer.WriteInt(1));
        }

        [Test]
        public void WriteIntProperlyUpdateBuffer()
        {
            const int input = 12345;
            var expected = BitConverter.GetBytes(input);

            _byteRingBuffer.WriteInt(input);
            for (int i = 0; i < sizeof(int); i++)
            {
                Assert.AreEqual(expected[i], _byteRingBuffer[i]);
            }
        }

        [Test]
        public void WriteIntShouldRaiseFullEventWhenBufferWrap()
        {
            int eventRaisedCount = 0;
            _byteRingBuffer.BufferFull +=
                () =>
                {
                    eventRaisedCount++;
                };
            _byteRingBuffer.Position = BufferSize - 2;

            _byteRingBuffer.WriteInt(1);

            Assert.AreEqual(2, _byteRingBuffer.Position);
            Assert.AreEqual(1, eventRaisedCount);
        }

        [Test]
        public void WriteBytesShouldUpdatePosition()
        {
            const int expectedPosition = 5;
            var input = new byte[expectedPosition];

            _byteRingBuffer.WriteBytes(new ArraySegment<byte>(input));
            Assert.AreEqual(expectedPosition, _byteRingBuffer.Position);
        }

        [Test]
        public void WriteBytesReturnsSameInstance()
        {
            Assert.AreEqual(_byteRingBuffer, _byteRingBuffer.WriteBytes(new ArraySegment<byte>(new byte[5])));
        }

        [Test]
        public void WriteBytesProperlyUpdatesBuffer()
        {
            byte[] bytes = {1, 2, 3, 4};

            _byteRingBuffer.WriteBytes(new ArraySegment<byte>(bytes));
            for (int i = 0; i < bytes.Length; i++)
            {
                Assert.AreEqual(bytes[i], _byteRingBuffer[i]);
            }
        }

        [Test]
        public void WriteBytesRaisesBufferFullEventWhenWraps()
        {
            int eventRaisedCount = 0;
            _byteRingBuffer.BufferFull +=
                () =>
                {
                    eventRaisedCount++;
                };

            var bytes = new byte[35];

            _byteRingBuffer.WriteBytes(new ArraySegment<byte>(bytes));
            Assert.AreEqual(3, eventRaisedCount);
        }
    }
}