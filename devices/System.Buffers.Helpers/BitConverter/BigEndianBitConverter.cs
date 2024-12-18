// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;

namespace Iot.Device.MulticastDns.Package
{
    /// <summary>
    /// The <see cref="BigEndianBitConverter"/> class offers methods to convert to and from a byte[] in a Big Endian manner.
    /// </summary>
    public sealed class BigEndianBitConverter : EndianBitConverter
    {
        /// <summary>
        /// Indicates if this converter instance is Little Endian.
        /// </summary>
        public override bool IsLittleEndian => false;

        /// <summary>
        /// Returns a byte[] representation of a <see cref="char"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The byte[] with the converted value.</returns>
        public override byte[] GetBytes(char value)
        {
            byte[] result = new byte[2];
            BinaryPrimitives.WriteInt16BigEndian(result, (short)value);
            return result;
        }

        /// <summary>
        /// Returns a byte[] representation of a <see cref="short"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The byte[] with the converted value.</returns>
        public override byte[] GetBytes(short value)
        {
            byte[] result = new byte[2];
            BinaryPrimitives.WriteInt16BigEndian(result, value);
            return result;
        }

        /// <summary>
        /// Returns a byte[] representation of a <see cref="int"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The byte[] with the converted value.</returns>
        public override byte[] GetBytes(int value)
        {
            byte[] result = new byte[4];
            BinaryPrimitives.WriteInt32BigEndian(result, value);
            return result;
        }

        /// <summary>
        /// Returns a byte[] representation of a <see cref="long"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The byte[] with the converted value.</returns>
        public override byte[] GetBytes(long value)
        {
            byte[] result = new byte[8];
            BinaryPrimitives.WriteInt64BigEndian(result, value);
            return result;
        }

        /// <summary>
        /// Returns a byte[] representation of a <see cref="ushort"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The byte[] with the converted value.</returns>
        public override byte[] GetBytes(ushort value)
        {
            byte[] result = new byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(result, value);
            return result;
        }

        /// <summary>
        /// Returns a byte[] representation of a <see cref="uint"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The byte[] with the converted value.</returns>
        public override byte[] GetBytes(uint value)
        {
            byte[] result = new byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(result, value);
            return result;
        }

        /// <summary>
        /// Returns a byte[] representation of a <see cref="ulong"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The byte[] with the converted value.</returns>
        public override byte[] GetBytes(ulong value)
        {
            byte[] result = new byte[8];
            BinaryPrimitives.WriteUInt64BigEndian(result, value);
            return result;
        }

        /// <summary>
        /// Returns a byte[] representation of a <see cref="float"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The byte[] with the converted value.</returns>
        public override byte[] GetBytes(float value)
        {
            byte[] result = new byte[4];
            BinaryPrimitives.WriteSingleBigEndian(result, value);
            return result;
        }

        /// <summary>
        /// Returns a byte[] representation of a <see cref="double"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The byte[] with the converted value.</returns>
        public override byte[] GetBytes(double value)
        {
            byte[] result = new byte[8];
            BinaryPrimitives.WriteDoubleBigEndian(result, value);
            return result;
        }

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="char"/>.
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        public override char ToChar(SpanByte value, int index = 0)
        {
            return unchecked((char)BinaryPrimitives.ReadInt16BigEndian(value.Slice(index, 2)));
        }

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="short"/>.
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        public override short ToInt16(SpanByte value, int index = 0)
        {
            return BinaryPrimitives.ReadInt16BigEndian(value.Slice(index, 2));
        }

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="int"/>.
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        public override int ToInt32(SpanByte value, int index = 0)
        {
            return BinaryPrimitives.ReadInt32BigEndian(value.Slice(index, 4));
        }

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="long"/>.
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        public override long ToInt64(SpanByte value, int index = 0)
        {
            return BinaryPrimitives.ReadInt64BigEndian(value.Slice(index, 8));
        }

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="ushort"/>.
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        public override ushort ToUInt16(SpanByte value, int index = 0)
        {
            return BinaryPrimitives.ReadUInt16BigEndian(value.Slice(index, 2));
        }

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="uint"/>.
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        public override uint ToUInt32(SpanByte value, int index = 0)
        {
            return BinaryPrimitives.ReadUInt32BigEndian(value.Slice(index, 4));
        }

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="ulong"/>.
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        public override ulong ToUInt64(SpanByte value, int index = 0)
        {
            return BinaryPrimitives.ReadUInt64BigEndian(value.Slice(index, 8));
        }

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="float"/>.
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        public override float ToSingle(SpanByte value, int index = 0)
        {
            return BinaryPrimitives.ReadSingleBigEndian(value.Slice(index, 4));
        }

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="double"/>.
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        public override double ToDouble(SpanByte value, int index = 0)
        {
            return BinaryPrimitives.ReadDoubleBigEndian(value.Slice(index, 8));
        }
    }
}
