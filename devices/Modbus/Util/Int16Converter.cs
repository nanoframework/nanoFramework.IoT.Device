// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Modbus.Util
{
    /// <summary>
    /// Provides utility methods for converting between different data types and Modbus-specific formats.
    /// </summary>
    public static class Int16Converter
    {
        /// <summary>
        /// Converts an array of bytes to an array of unsigned 16-bit integers.
        /// </summary>
        /// <param name="values">The array of bytes to convert.</param>
        /// <param name="count">The number of bytes to convert. If -1, all bytes will be converted.</param>
        /// <returns>An array of unsigned 16-bit integers.</returns>
        public static ushort[] From(byte[] values, int count = -1)
        {
            if (values == null || values.Length == 0 || count == 0)
            {
                return new ushort[0];
            }

            if (count > values.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (count < 0)
            {
                count = values.Length;
            }

            ushort[] shorts = new ushort[count / 2];
            for (int i = 0; i < shorts.Length; i++)
            {
                var tmps = new byte[] { values[i * 2], values[(i * 2) + 1] };
                tmps.JudgReverse();

                shorts[i] = BitConverter.ToUInt16(tmps, 0);
            }

            return shorts;
        }

        /// <summary>
        /// Converts an array of unsigned 16-bit integers to an array of bytes.
        /// </summary>
        /// <param name="values">The array of unsigned 16-bit integers to convert.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] ToBytes(params ushort[] values)
        {
            if (values == null || values.Length == 0)
            {
                return new byte[0];
            }

            byte[] bytes = new byte[values.Length * 2];
            for (int i = 0; i < values.Length; i++)
            {
                byte[] blob = BitConverter.GetBytes(values[i]);
                blob.JudgReverse();

                var idx = i * 2;
                bytes[idx] = blob[0];
                bytes[idx + 1] = blob[1];
            }

            return bytes;
        }

        /// <summary>
        /// Converts an integer value to an unsigned 16-bit integer.
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
        /// <returns>An unsigned 16-bit integer.</returns>
        public static ushort From(int value)
        {
            if (value > short.MaxValue || value < short.MinValue)
            {
                throw new ArgumentException(nameof(value));
            }

            if (value >= 0)
            {
                return (ushort)value;
            }
            else
            {
                return (ushort)ComplementCode(value);
            }
        }

        /// <summary>
        /// Converts an unsigned 16-bit integer to an integer value.
        /// </summary>
        /// <param name="value">The unsigned 16-bit integer to convert.</param>
        /// <returns>An integer value.</returns>
        public static int ToInt32(ushort value)
        {
            return value - ((value > short.MaxValue) ? (ushort.MaxValue + 1) : 0);
        }

        /// <summary>
        /// Converts a <see cref="TimeSpan"/> value to an unsigned 16-bit integer containing the hours and minutes.
        /// </summary>
        /// <param name="time">The <see cref="TimeSpan"/> value to convert.</param>
        /// <returns>An unsigned 16-bit integer containing the hours and minutes.</returns>
        public static ushort From(TimeSpan time)
        {
            byte bit1 = (byte)time.Hours;
            byte bit2 = (byte)time.Minutes;

            return From(new byte[] { bit1, bit2 })[0];
        }

        /// <summary>
        /// Converts an unsigned 16-bit integer containing the hours and minutes to a <see cref="TimeSpan"/> value.
        /// </summary>
        /// <param name="value">The unsigned 16-bit integer containing the hours and minutes to convert.</param>
        /// <returns>A <see cref="TimeSpan"/> value.</returns>
        public static TimeSpan ToTime(ushort value)
        {
            var bytes = ToBytes(value);
            var h = bytes[0] <= 23 ? bytes[0] : 23;
            var m = bytes[1] <= 59 ? bytes[1] : 59;

            return new TimeSpan(h, m, 0);
        }

        /// <summary>
        /// Converts a <see cref="DateTime"/> value to an array of unsigned 16-bit integers.
        /// </summary>
        /// <param name="dt">The <see cref="DateTime"/> value to convert.</param>
        /// <returns>An array of unsigned 16-bit integers.</returns>
        public static ushort[] From(DateTime dt)
        {
            var val = dt.Subtract(DateTime.UnixEpoch);
            var bytes = BitConverter.GetBytes(val.Ticks);

            return From(bytes);
        }

        /// <summary>
        /// Converts an array of unsigned 16-bit integers to a <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="values">The array of unsigned 16-bit integers to convert.</param>
        /// <returns>A <see cref="DateTime"/> value.</returns>
        public static DateTime ToDateTime(ushort[] values)
        {
            if (values == null || values.Length < 4)
            {
                return DateTime.MinValue;
            }

            var bytes = ToBytes(values);
            var val = BitConverter.ToInt64(bytes, 0);

            return DateTime.UnixEpoch.AddTicks(val);
        }

        /// <summary>
        /// Converts a string to an array of unsigned 16-bit integers.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <param name="start">The starting position in the string.</param>
        /// <param name="length">The number of characters to convert. If -1, all characters from the starting position will be converted.</param>
        /// <returns>An array of unsigned 16-bit integers.</returns>
        public static ushort[] From(string s, int start = 0, int length = -1)
        {
            if (!string.IsNullOrEmpty(s))
            {
                if (length == -1)
                {
                    length = s.Length - start;
                }

                byte[] bytes = new byte[length * 2];
                var count = System.Text.Encoding.UTF8.GetBytes(s, start, length, bytes, 0);
                if (count > 0)
                {
                    return From(bytes, count);
                }
            }

            return new ushort[0];
        }

        /// <summary>
        /// Converts an array of unsigned 16-bit integers to a string.
        /// </summary>
        /// <param name="values">The array of unsigned 16-bit integers to convert.</param>
        /// <returns>A string.</returns>
        public static string ToString(ushort[] values)
        {
            if (values == null || values.Length < 0)
            {
                return string.Empty;
            }

            var bytes = ToBytes(values);
            return System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        // 计算补码
        private static int ComplementCode(int original)
        {
            int a = short.MaxValue;
            int b = short.MinValue;
            int c = a - b;
            int d;
            if (original > 0)
            {
                d = -(c - original + 1);
            }
            else
            {
                d = c + original + 1;
            }

            return d;
        }
    }
}
