// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.MulticastDns.Package
{
    /// <summary>
    /// The <see cref="IBitConverter"/> interface offers methods to convert from and to a byte[].
    /// </summary>
    public interface IBitConverter
    {
        /// <summary>
        /// Returns a byte[] representation of a <see cref="bool"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The byte[] with the converted value.</returns>
        byte[] GetBytes(bool value);

        /// <summary>
        /// Returns a byte[] representation of a <see cref="char"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The byte[] with the converted value.</returns>
        byte[] GetBytes(char value);

        /// <summary>
        /// Returns a byte[] representation of a <see cref="short"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The byte[] with the converted value.</returns>
        byte[] GetBytes(short value);

        /// <summary>
        /// Returns a byte[] representation of a <see cref="int"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The byte[] with the converted value.</returns>
        byte[] GetBytes(int value);

        /// <summary>
        /// Returns a byte[] representation of a <see cref="long"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The byte[] with the converted value.</returns>
        byte[] GetBytes(long value);

        /// <summary>
        /// Returns a byte[] representation of a <see cref="ushort"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The byte[] with the converted value.</returns>
        byte[] GetBytes(ushort value);

        /// <summary>
        /// Returns a byte[] representation of a <see cref="uint"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The byte[] with the converted value.</returns>
        byte[] GetBytes(uint value);

        /// <summary>
        /// Returns a byte[] representation of a <see cref="ulong"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The byte[] with the converted value.</returns>
        byte[] GetBytes(ulong value);

        /// <summary>
        /// Returns a byte[] representation of a <see cref="float"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The byte[] with the converted value.</returns>
        byte[] GetBytes(float value);

        /// <summary>
        /// Returns a byte[] representation of a <see cref="double"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The byte[] with the converted value.</returns>
        byte[] GetBytes(double value);

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="char"/>.
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        char ToChar(SpanByte value, int index = 0);

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="short"/>.
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        short ToInt16(SpanByte value, int index = 0);

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="int"/>.
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        int ToInt32(SpanByte value, int index = 0);

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="long"/>.
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        long ToInt64(SpanByte value, int index = 0);

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="ushort"/>.
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        ushort ToUInt16(SpanByte value, int index = 0);

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="uint"/>.
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        uint ToUInt32(SpanByte value, int index = 0);

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="ulong"/>.
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        ulong ToUInt64(SpanByte value, int index = 0);

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="float"/>.
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        float ToSingle(SpanByte value, int index = 0);

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="double"/>.
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        double ToDouble(SpanByte value, int index = 0);

        /// <summary>
        /// Converts a <see cref="SpanByte"/> to a <see cref="bool"/>.
        /// </summary>
        /// <param name="value">The <see cref="SpanByte"/> that points to the data to convert.</param>
        /// <param name="index">The index where the data can be found.</param>
        /// <returns>The converted data.</returns>
        bool ToBoolean(SpanByte value, int index = 0);
    }
}
