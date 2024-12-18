// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers.Binary;

namespace System.Buffers.Helpers.BitConverter
{
    /// <summary>
    /// The <see cref="LittleEndianBitConverter"/> class offers methods to convert to and from a byte[] in a Little Endian manner.
    /// </summary>
    public sealed class LittleEndianBitConverter : EndianBitConverter
    {
        /// <summary>
        /// Indicates if this converter instance is Little Endian.
        /// </summary>
        public override bool IsLittleEndian => true;

        /// <summary>
        /// Returns a byte[] representation of a <see cref="char"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The byte[] with the converted value.</returns>
        public override byte[] GetBytes(char value)
        {
            byte[] result = new byte[2];
            BinaryPrimitives.WriteInt16LittleEndian(result, (short)value);
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
            BinaryPrimitives.WriteInt16LittleEndian(result, value);
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
            BinaryPrimitives.WriteInt32LittleEndian(result, value);
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
            BinaryPrimitives.WriteInt64LittleEndian(result, value);
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
            BinaryPrimitives.WriteUInt16LittleEndian(result, value);
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
            BinaryPrimitives.WriteUInt32LittleEndian(result, value);
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
            BinaryPrimitives.WriteUInt64LittleEndian(result, value);
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
            BinaryPrimitives.WriteSingleLittleEndian(result, value);
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
            BinaryPrimitives.WriteDoubleLittleEndian(result, value);
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
            return unchecked((char)BinaryPrimitives.ReadInt16LittleEndian(value.Slice(index, 2)));
        }

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="short"/>.
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        public override short ToInt16(SpanByte value, int index = 0)
        {
            return BinaryPrimitives.ReadInt16LittleEndian(value.Slice(index, 2));
        }

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="int"/>.
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        public override int ToInt32(SpanByte value, int index = 0)
        {
            return BinaryPrimitives.ReadInt32LittleEndian(value.Slice(index, 4));
        }

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="long"/>.
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        public override long ToInt64(SpanByte value, int index = 0)
        {
            return BinaryPrimitives.ReadInt64LittleEndian(value.Slice(index, 8));
        }

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="ushort"/>.
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        public override ushort ToUInt16(SpanByte value, int index = 0)
        {
            return BinaryPrimitives.ReadUInt16LittleEndian(value.Slice(index, 2));
        }

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="uint"/>.
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        public override uint ToUInt32(SpanByte value, int index = 0)
        {
            return BinaryPrimitives.ReadUInt32LittleEndian(value.Slice(index, 4));
        }

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="ulong"/>.
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        public override ulong ToUInt64(SpanByte value, int index = 0)
        {
            return BinaryPrimitives.ReadUInt64LittleEndian(value.Slice(index, 8));
        }

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="float"/>.
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        public override float ToSingle(SpanByte value, int index = 0)
        {
            return BinaryPrimitives.ReadSingleLittleEndian(value.Slice(index, 4));
        }

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="double"/>.
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        public override double ToDouble(SpanByte value, int index = 0)
        {
            return BinaryPrimitives.ReadDoubleLittleEndian(value.Slice(index, 8));
        }
    }
}
