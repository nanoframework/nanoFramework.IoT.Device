// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.MulticastDns.Package
{
    /// <summary>
    /// The <see cref="EndianBitConverter"/> class is the base class for Little and Big Endian Converters.
    /// </summary>
    public abstract class EndianBitConverter : IBitConverter
    {
        /// <summary>
        /// Gets a static instance of a <see cref="BigEndianBitConverter"/>.
        /// </summary>
        public static IBitConverter Big
        {
            get
            {
                if (_big == null)
                {
                    _big = new BigEndianBitConverter();
                }

                return _big;
            }
        }

        /// <summary>
        /// Gets a static instance of a <see cref="LittleEndianBitConverter"/>
        /// </summary>
        public static IBitConverter Little
        {
            get
            {
                if (_little == null)
                {
                    _little = new LittleEndianBitConverter();
                }

                return _little;
            }
        }

        private static BigEndianBitConverter _big;

        private static LittleEndianBitConverter _little;

        /// <summary>
        /// Gets a value indicating whether this converter instance is Little Endian.
        /// </summary>
        public abstract bool IsLittleEndian { get; }

        /// <summary>
        /// Returns a byte[] representation of a <see cref="bool"/>
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The byte[] with the converted value.</returns>
        public byte[] GetBytes(bool value)
        {
            return new byte[] { value ? (byte)0x1 : (byte)0x0 };
        }

        /// <summary>
        /// Returns a byte[] representation of a <see cref="char"/>
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The byte[] with the converted value.</returns>
        public abstract byte[] GetBytes(char value);

        /// <summary>
        /// Returns a byte[] representation of a <see cref="short"/>
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The byte[] with the converted value.</returns>
        public abstract byte[] GetBytes(short value);

        /// <summary>
        /// Returns a byte[] representation of a <see cref="int"/>
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The byte[] with the converted value.</returns>
        public abstract byte[] GetBytes(int value);

        /// <summary>
        /// Returns a byte[] representation of a <see cref="long"/>
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The byte[] with the converted value.</returns>
        public abstract byte[] GetBytes(long value);

        /// <summary>
        /// Returns a byte[] representation of a <see cref="ushort"/>
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The byte[] with the converted value.</returns>
        public abstract byte[] GetBytes(ushort value);

        /// <summary>
        /// Returns a byte[] representation of a <see cref="uint"/>
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The byte[] with the converted value.</returns>
        public abstract byte[] GetBytes(uint value);

        /// <summary>
        /// Returns a byte[] representation of a <see cref="ulong"/>
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The byte[] with the converted value.</returns>
        public abstract byte[] GetBytes(ulong value);

        /// <summary>
        /// Returns a byte[] representation of a <see cref="float"/>
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The byte[] with the converted value.</returns>
        public abstract byte[] GetBytes(float value);

        /// <summary>
        /// Returns a byte[] representation of a <see cref="double"/>
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The byte[] with the converted value.</returns>
        public abstract byte[] GetBytes(double value);

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="bool"/>
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        public bool ToBoolean(SpanByte value, int index = 0) => value[index] == 0x1;

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="char"/>
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        public abstract char ToChar(SpanByte value, int index = 0);

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="short"/>
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        public abstract short ToInt16(SpanByte value, int index = 0);

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="int"/>
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        public abstract int ToInt32(SpanByte value, int index = 0);

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="long"/>
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        public abstract long ToInt64(SpanByte value, int index = 0);

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="ushort"/>
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        public abstract ushort ToUInt16(SpanByte value, int index = 0);

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="uint"/>
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        public abstract uint ToUInt32(SpanByte value, int index = 0);

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="ulong"/>
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        public abstract ulong ToUInt64(SpanByte value, int index = 0);

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="float"/>
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        public abstract float ToSingle(SpanByte value, int index = 0);

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="double"/>
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        public abstract double ToDouble(SpanByte value, int index = 0);
    }
}
