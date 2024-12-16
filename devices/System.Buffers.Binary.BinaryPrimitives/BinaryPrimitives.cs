// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.

namespace System.Buffers.Binary
{
    /// <summary>
    /// Reads bytes as primitives with specific endianness.
    /// </summary>
    public static class BinaryPrimitives
    {
        /// <summary>
        /// Reads an System.Int16 from the beginning of a read-only span of bytes, as big endian.
        /// </summary>
        /// <param name="source">The read-only span to read.</param>
        /// <returns>The big endian value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Source is too small to contain an System.Int16.</exception>
        public static short ReadInt16BigEndian(SpanByte source)
        {
            if (source.Length < 2)
            {
                throw new ArgumentOutOfRangeException();
            }

            return (short)(source[0] << 8 | source[1]);
        }

        /// <summary>
        /// Reads an System.Int16 from the beginning of a read-only span of bytes, as little endian.
        /// </summary>
        /// <param name="source">The read-only span to read.</param>
        /// <returns>The little endian value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Source is too small to contain an System.Int16.</exception>
        public static short ReadInt16LittleEndian(SpanByte source)
        {
            if (source.Length < 2)
            {
                throw new ArgumentOutOfRangeException();
            }

            return (short)(source[1] << 8 | source[0]);
        }

        /// <summary>
        /// Reads an System.Int32 from the beginning of a read-only span of bytes, as big endian.
        /// </summary>
        /// <param name="source">The read-only span to read.</param>
        /// <returns>The big endian value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Source is too small to contain an System.Int32.</exception>
        public static int ReadInt32BigEndian(SpanByte source)
        {
            if (source.Length < 4)
            {
                throw new ArgumentOutOfRangeException();
            }

            return (int)(source[0] << 24 | source[1] << 16 | source[2] << 8 | source[3]);
        }

        /// <summary>
        /// Reads an System.Int32 from the beginning of a read-only span of bytes, as little endian.
        /// </summary>
        /// <param name="source">The read-only span to read.</param>
        /// <returns>The little endian value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Source is too small to contain an System.Int32.</exception>
        public static int ReadInt32LittleEndian(SpanByte source)
        {
            if (source.Length < 4)
            {
                throw new ArgumentOutOfRangeException();
            }

            return (int)(source[3] << 24 | source[2] << 16 | source[1] << 8 | source[0]);
        }

        /// <summary>
        /// Reads an System.Int64 from the beginning of a read-only span of bytes, as little endian.
        /// </summary>
        /// <param name="source">The read-only span to read.</param>
        /// <returns>The little endian value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Source is too small to contain an System.Int64.</exception>
        public static long ReadInt64BigEndian(SpanByte source)
        {
            if (source.Length < 8)
            {
                throw new ArgumentOutOfRangeException();
            }

            return (long)source[0] << 56 | (long)source[1] << 48 | (long)source[2] << 40 | (long)source[3] << 32 |
                (long)source[4] << 24 | (long)source[5] << 16 | (long)source[6] << 8 | (long)source[7];
        }

        /// <summary>
        /// Reads an System.Int64 from the beginning of a read-only span of bytes, as big endian.
        /// </summary>
        /// <param name="source">The read-only span to read.</param>
        /// <returns>The big endian value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Source is too small to contain an System.Int64.</exception>
        public static long ReadInt64LittleEndian(SpanByte source)
        {
            if (source.Length < 8)
            {
                throw new ArgumentOutOfRangeException();
            }

            return (long)source[7] << 56 | (long)source[6] << 48 | (long)source[5] << 40 | (long)source[4] << 32 |
                (long)source[3] << 24 | (long)source[2] << 16 | (long)source[1] << 8 | (long)source[0];
        }

        /// <summary>
        /// Reads a System.UInt16 from the beginning of a read-only span of bytes, as big  endian.
        /// </summary>
        /// <param name="source">The read-only span to read.</param>
        /// <returns>The big endian value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Source is too small to contain an System.Int16.</exception>
        public static ushort ReadUInt16BigEndian(SpanByte source)
        {
            if (source.Length < 2)
            {
                throw new ArgumentOutOfRangeException();
            }

            return (ushort)(source[0] << 8 | source[1]);
        }

        /// <summary>
        /// Reads a System.UInt16 from the beginning of a read-only span of bytes, as little endian.
        /// </summary>
        /// <param name="source">The read-only span to read.</param>
        /// <returns>The little endian value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Source is too small to contain an System.Int16.</exception>
        public static ushort ReadUInt16LittleEndian(SpanByte source)
        {
            if (source.Length < 2)
            {
                throw new ArgumentOutOfRangeException();
            }

            return (ushort)(source[1] << 8 | source[0]);
        }

        /// <summary>
        /// Reads a System.UInt32 from the beginning of a read-only span of bytes, as big endian.
        /// </summary>
        /// <param name="source">The read-only span to read.</param>
        /// <returns> The big endian value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Source is too small to contain an System.Int32.</exception>
        public static uint ReadUInt32BigEndian(SpanByte source)
        {
            if (source.Length < 4)
            {
                throw new ArgumentOutOfRangeException();
            }

            return (uint)(source[0] << 24 | source[1] << 16 | source[2] << 8 | source[3]);
        }

        /// <summary>
        /// Reads a System.UInt32 from the beginning of a read-only span of bytes, as little endian.
        /// </summary>
        /// <param name="source">The read-only span of bytes to read.</param>
        /// <returns>The little endian value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Source is too small to contain an System.Int32.</exception>
        public static uint ReadUInt32LittleEndian(SpanByte source)
        {
            if (source.Length < 4)
            {
                throw new ArgumentOutOfRangeException();
            }

            return (uint)(source[3] << 24 | source[2] << 16 | source[1] << 8 | source[0]);
        }

        /// <summary>
        /// Reads a System.UInt64 from the beginning of a read-only span of bytes, as big  endian.
        /// </summary>
        /// <param name="source">The read-only span of bytes to read.</param>
        /// <returns>The big endian value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Source is too small to contain an System.Int64.</exception>
        public static ulong ReadUInt64BigEndian(SpanByte source)
        {
            if (source.Length < 8)
            {
                throw new ArgumentOutOfRangeException();
            }

            return (ulong)source[0] << 56 | (ulong)source[1] << 48 | (ulong)source[2] << 40 | (ulong)source[3] << 32 |
                (ulong)source[4] << 24 | (ulong)source[5] << 16 | (ulong)source[6] << 8 | (ulong)source[7];
        }

        /// <summary>
        /// Reads a System.UInt64 from the beginning of a read-only span of bytes, as little endian.
        /// </summary>
        /// <param name="source">The read-only span of bytes to read.</param>
        /// <returns>The little endian value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Source is too small to contain an System.Int64.</exception>
        public static ulong ReadUInt64LittleEndian(SpanByte source)
        {
            if (source.Length < 8)
            {
                throw new ArgumentOutOfRangeException();
            }

            return (ulong)source[7] << 56 | (ulong)source[6] << 48 | (ulong)source[5] << 40 | (ulong)source[4] << 32 |
                (ulong)source[3] << 24 | (ulong)source[2] << 16 | (ulong)source[1] << 8 | (ulong)source[0];
        }

        /// <summary>
        /// Reads a <see cref="float"/> from the beginning of a read-only span of bytes, as big endian.
        /// </summary>
        /// <param name="source">The read-only span to read.</param>
        /// <returns>The big endian value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Source is too small to contain a <see cref="float"/>.</exception>
        public static float ReadSingleBigEndian(SpanByte source)
        {
            if (source.Length < 4)
            {
                throw new ArgumentOutOfRangeException();
            }

            uint ieee754_bits = ReadUInt32BigEndian(source);
            float flt;
            unsafe
            {
                *(IntPtr*)&flt = *(IntPtr*)&ieee754_bits; // https://stackoverflow.com/a/57532166/281337
            }

            // This assignment is required. Without it, the CLR will continue to use the ieee754_bits value (but only in Release builds).
            float converted = flt;
            return converted;
        }

        /// <summary>
        /// Reads a <see cref="float"/> from the beginning of a read-only span of bytes, as little endian.
        /// </summary>
        /// <param name="source">The read-only span to read.</param>
        /// <returns>The little endian value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Source is too small to contain a <see cref="float"/>.</exception>
        public static float ReadSingleLittleEndian(SpanByte source)
        {
            if (source.Length < 4)
            {
                throw new ArgumentOutOfRangeException();
            }

            uint ieee754_bits = ReadUInt32LittleEndian(source);
            float flt;
            unsafe
            {
                *(IntPtr*)&flt = *(IntPtr*)&ieee754_bits; // https://stackoverflow.com/a/57532166/281337
            }

            // This assignment is required. Without it, the CLR will continue to use the ieee754_bits value (but only in Release builds).
            float converted = flt;
            return converted;
        }

        /// <summary>
        /// Reads a <see cref="double"/> from the beginning of a read-only span of bytes, as big endian.
        /// </summary>
        /// <param name="source">The read-only span to read.</param>
        /// <returns>The big endian value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Source is too small to contain a <see cref="double"/>.</exception>
        public static double ReadDoubleBigEndian(SpanByte source)
        {
            if (source.Length < 8)
            {
                throw new ArgumentOutOfRangeException();
            }

            long bits = ReadInt64BigEndian(source);
            double dbl;
            unsafe
            {
                dbl = *(double*)&bits;
            }

            return dbl;
        }

        /// <summary>
        /// Reads a <see cref="double"/> from the beginning of a read-only span of bytes, as little endian.
        /// </summary>
        /// <param name="source">The read-only span to read.</param>
        /// <returns>The little endian value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Source is too small to contain a <see cref="double"/>.</exception>
        public static double ReadDoubleLittleEndian(SpanByte source)
        {
            if (source.Length < 8)
            {
                throw new ArgumentOutOfRangeException();
            }

            long bits = ReadInt64LittleEndian(source);
            double dbl;
            unsafe
            {
                dbl = *(double*)&bits;
            }

            return dbl;
        }

        /// <summary>
        /// Writes an System.Int16 into a span of bytes, as big endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as big endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException">Source is too small to contain an System.Int16.</exception>
        public static void WriteInt16BigEndian(SpanByte destination, short value)
        {
            if (destination.Length < 2)
            {
                throw new ArgumentOutOfRangeException();
            }

            destination[0] = (byte)(value >> 8);
            destination[1] = (byte)value;
        }

        /// <summary>
        /// Writes an System.Int16 into a span of bytes, as little endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException">Source is too small to contain an System.Int16.</exception>
        public static void WriteInt16LittleEndian(SpanByte destination, short value)
        {
            if (destination.Length < 2)
            {
                throw new ArgumentOutOfRangeException();
            }

            destination[1] = (byte)(value >> 8);
            destination[0] = (byte)value;
        }

        /// <summary>
        /// Writes an System.Int32 into a span of bytes, as big endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as big endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException">Source is too small to contain an System.Int32.</exception>
        public static void WriteInt32BigEndian(SpanByte destination, int value)
        {
            if (destination.Length < 4)
            {
                throw new ArgumentOutOfRangeException();
            }

            destination[0] = (byte)(value >> 24);
            destination[1] = (byte)(value >> 16);
            destination[2] = (byte)(value >> 8);
            destination[3] = (byte)value;
        }

        /// <summary>
        /// Writes an System.Int32 into a span of bytes, as little endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException">Source is too small to contain an System.Int32.</exception>
        public static void WriteInt32LittleEndian(SpanByte destination, int value)
        {
            if (destination.Length < 4)
            {
                throw new ArgumentOutOfRangeException();
            }

            destination[3] = (byte)(value >> 24);
            destination[2] = (byte)(value >> 16);
            destination[1] = (byte)(value >> 8);
            destination[0] = (byte)value;
        }

        /// <summary>
        /// Writes an System.Int64 into a span of bytes, as big endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as big endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException">Source is too small to contain an System.Int64.</exception>
        public static void WriteInt64BigEndian(SpanByte destination, long value)
        {
            if (destination.Length < 8)
            {
                throw new ArgumentOutOfRangeException();
            }

            destination[0] = (byte)(value >> 56);
            destination[1] = (byte)(value >> 48);
            destination[2] = (byte)(value >> 40);
            destination[3] = (byte)(value >> 32);
            destination[4] = (byte)(value >> 24);
            destination[5] = (byte)(value >> 16);
            destination[6] = (byte)(value >> 8);
            destination[7] = (byte)value;
        }

        /// <summary>
        /// Writes an System.Int64 into a span of bytes, as little endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException">Source is too small to contain an System.Int64.</exception>
        public static void WriteInt64LittleEndian(SpanByte destination, long value)
        {
            if (destination.Length < 8)
            {
                throw new ArgumentOutOfRangeException();
            }

            destination[7] = (byte)(value >> 56);
            destination[6] = (byte)(value >> 48);
            destination[5] = (byte)(value >> 40);
            destination[4] = (byte)(value >> 32);
            destination[3] = (byte)(value >> 24);
            destination[2] = (byte)(value >> 16);
            destination[1] = (byte)(value >> 8);
            destination[0] = (byte)value;
        }

        /// <summary>
        /// Writes a System.UInt16 into a span of bytes, as big endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as big endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException">Source is too small to contain an System.Int16.</exception>
        public static void WriteUInt16BigEndian(SpanByte destination, ushort value)
        {
            if (destination.Length < 2)
            {
                throw new ArgumentOutOfRangeException();
            }

            destination[0] = (byte)(value >> 8);
            destination[1] = (byte)value;
        }

        /// <summary>
        /// Writes a System.UInt16 into a span of bytes, as little endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException">Source is too small to contain an System.Int16.</exception>
        public static void WriteUInt16LittleEndian(SpanByte destination, ushort value)
        {
            if (destination.Length < 2)
            {
                throw new ArgumentOutOfRangeException();
            }

            destination[1] = (byte)(value >> 8);
            destination[0] = (byte)value;
        }

        /// <summary>
        /// Writes a System.UInt32 into a span of bytes, as big endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as big endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException">Source is too small to contain an System.Int32.</exception>
        public static void WriteUInt32BigEndian(SpanByte destination, uint value)
        {
            if (destination.Length < 4)
            {
                throw new ArgumentOutOfRangeException();
            }

            destination[0] = (byte)(value >> 24);
            destination[1] = (byte)(value >> 16);
            destination[2] = (byte)(value >> 8);
            destination[3] = (byte)value;
        }

        /// <summary>
        /// Writes a System.UInt32 into a span of bytes, as little endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException">Source is too small to contain an System.Int32.</exception>
        public static void WriteUInt32LittleEndian(SpanByte destination, uint value)
        {
            if (destination.Length < 4)
            {
                throw new ArgumentOutOfRangeException();
            }

            destination[3] = (byte)(value >> 24);
            destination[2] = (byte)(value >> 16);
            destination[1] = (byte)(value >> 8);
            destination[0] = (byte)value;
        }

        /// <summary>
        /// Writes a System.UInt64 into a span of bytes, as big endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as big endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException">Source is too small to contain an System.Int64.</exception>
        public static void WriteUInt64BigEndian(SpanByte destination, ulong value)
        {
            if (destination.Length < 8)
            {
                throw new ArgumentOutOfRangeException();
            }

            destination[0] = (byte)(value >> 56);
            destination[1] = (byte)(value >> 48);
            destination[2] = (byte)(value >> 40);
            destination[3] = (byte)(value >> 32);
            destination[4] = (byte)(value >> 24);
            destination[5] = (byte)(value >> 16);
            destination[6] = (byte)(value >> 8);
            destination[7] = (byte)value;
        }

        /// <summary>
        /// Writes a System.UInt64 into a span of bytes, as little endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException">Source is too small to contain an System.Int64.</exception>
        public static void WriteUInt64LittleEndian(SpanByte destination, ulong value)
        {
            if (destination.Length < 8)
            {
                throw new ArgumentOutOfRangeException();
            }

            destination[7] = (byte)(value >> 56);
            destination[6] = (byte)(value >> 48);
            destination[5] = (byte)(value >> 40);
            destination[4] = (byte)(value >> 32);
            destination[3] = (byte)(value >> 24);
            destination[2] = (byte)(value >> 16);
            destination[1] = (byte)(value >> 8);
            destination[0] = (byte)value;
        }

        /// <summary>
        /// Writes a <see cref="float"/> into a span of bytes, as big endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as big endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException">Destination is too small to contain a <see cref="float"/>.</exception>
        public static void WriteSingleBigEndian(SpanByte destination, float value)
        {
            if (destination.Length < 4)
            {
                throw new ArgumentOutOfRangeException();
            }

            uint ieee754_bits;
            unsafe
            {
                *(IntPtr*)&ieee754_bits = *(IntPtr*)&value; // https://stackoverflow.com/a/57532166/281337
            }

            // This assignment is needed to prevent the CLR from throwing CLR_E_WRONG_TYPE when the next method performs the bitshifting.
            uint converted = ieee754_bits;
            WriteUInt32BigEndian(destination, converted);
        }

        /// <summary>
        /// Writes a <see cref="float"/> into a span of bytes, as little endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException">Destination is too small to contain a <see cref="float"/>.</exception>
        public static void WriteSingleLittleEndian(SpanByte destination, float value)
        {
            if (destination.Length < 4)
            {
                throw new ArgumentOutOfRangeException();
            }

            uint ieee754_bits;
            unsafe
            {
                *(IntPtr*)&ieee754_bits = *(IntPtr*)&value; // https://stackoverflow.com/a/57532166/281337
            }

            // This assignment is needed to prevent the CLR from throwing CLR_E_WRONG_TYPE when the next method performs the bitshifting.
            uint converted = ieee754_bits;
            WriteUInt32LittleEndian(destination, converted);
        }

        /// <summary>
        /// Writes a <see cref="double"/> into a span of bytes, as big endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as big endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException">Destination is too small to contain a <see cref="double"/>.</exception>
        public static void WriteDoubleBigEndian(SpanByte destination, double value)
        {
            if (destination.Length < 8)
            {
                throw new ArgumentOutOfRangeException();
            }

            long bits;
            unsafe
            {
                bits = *(long*)&value;
            }

            WriteInt64BigEndian(destination, bits);
        }

        /// <summary>
        /// Writes a <see cref="double"/> into a span of bytes, as little endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException">Destination is too small to contain a <see cref="double"/>.</exception>
        public static void WriteDoubleLittleEndian(SpanByte destination, double value)
        {
            if (destination.Length < 8)
            {
                throw new ArgumentOutOfRangeException();
            }

            long bits;
            unsafe
            {
                bits = *(long*)&value;
            }

            WriteInt64LittleEndian(destination, bits);
        }
    }
}