using System;

namespace Journaler
{
 
    ///<summary>
    /// A circular ring buffer of bytes
    ///</summary>
    public class ByteRingBuffer
    {
        private readonly byte[] _buffer;
        private const int SizeOfLong = sizeof (long);
        private const int SizeOfInt = sizeof (int);
        private int _position;

        /// <summary>
        /// Get the size of the buffer
        /// </summary>
        public int Size 
        {
            get { return _buffer.Length; }
        }

        /// <summary>
        /// Get the current position of the cursor within the buffer.
        /// Position is zero-based.
        /// Position goes back to 0 when the buffer wraps (ie. when <see cref="BufferFull"/> event is raised)
        /// </summary>
        public int Position
        {
            get { return _position; }
            set
            {
                if (value < 0 || value >= _buffer.Length) throw new ArgumentOutOfRangeException("value");
                _position = value;
            }
        }

        /// <summary>
        /// Event raise when the buffer wraps.
        /// When writing to the buffer, if there is not enough space left in the buffer to write 
        /// all bytes the write method will fill the remaining slots, raise the BufferFull event 
        /// and then write the next bytes at the begining of the buffer.
        /// </summary>
        public event Action BufferFull;

        /// <summary>
        /// Create a <see cref="ByteRingBuffer"/> with a given size
        /// </summary>
        /// <param name="size">number of bytes the buffer will contain</param>
        public ByteRingBuffer(int size)
        {
            if (size <= 0) throw new ArgumentOutOfRangeException("size");

            _buffer = new byte[size];
            Position = 0;
        }

        /// <summary>
        /// Write a single byte to the buffer.
        /// If this is the last byte written to the buffer the <see cref="BufferFull"/> event will be raised
        /// </summary>
        /// <param name="value">the value to write to the buffer</param>
        /// <returns>The same <see cref="ByteRingBuffer"/> so methods can be chained</returns>
        public ByteRingBuffer WriteByte(byte value)
        {
            _buffer[Position] = value;

            if(Position == Size - 1)
            {
                Position = 0;

                // wrap
                if(BufferFull != null)
                {
                    BufferFull();
                }
                return this;
            }

            Position++;

            return this;
        }

        /// <summary>
        /// Write an int to the buffer.
        /// If the buffer becomes full during the write the <see cref="BufferFull"/> will be raised 
        /// and then the remaining bytes will be written at the begining of the buffer.
        /// </summary>
        /// <param name="value">the value to write to the buffer</param>
        /// <returns>The same <see cref="ByteRingBuffer"/> so methods can be chained</returns>
        public ByteRingBuffer WriteInt(int value)
        {
            // TODO: BitConverter would allocate, perf test both options
            for (int i = 0; i < SizeOfInt; i++)
            {
                WriteByte((byte)(value >> (i * 8)));
            }
            return this;
        }

        /// <summary>
        /// Write a long to the buffer
        /// If the buffer becomes full during the write the <see cref="BufferFull"/> will be raised 
        /// and then the remaining bytes will be written at the begining of the buffer.
        /// </summary>
        /// <param name="value">the value to write to the buffer</param>
        /// <returns>The same <see cref="ByteRingBuffer"/> so methods can be chained</returns>
        public ByteRingBuffer WriteLong(long value)
        {
            // TODO: BitConverter would allocate, perf test both options
            for (int i = 0; i < SizeOfLong; i++)
            {
                WriteByte((byte) (value >> (i*8)));
            }
            return this;
        }

        /// <summary>
        /// Write an array of bytes to the buffer
        /// </summary>
        /// <param name="input">The array of byte to write from</param>
        /// <returns>The same <see cref="ByteRingBuffer"/> so methods can be chained</returns>
        public ByteRingBuffer WriteBytes(ArraySegment<byte> input)
        {
            for (int i = input.Offset; i < input.Offset + input.Count; i++)
            {
                WriteByte(input.Array[i]);
            }
            return this;
        }

        /// <summary>
        /// Read the byte at the given index
        /// </summary>
        /// <param name="index">index within the buffer</param>
        /// <returns>The value stored in the buffer at the provided index</returns>
        public byte this[int index]
        {
            get { return _buffer[index]; }
        }
    }
}