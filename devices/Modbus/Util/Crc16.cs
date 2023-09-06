// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Modbus.Util
{
    internal static class Crc16
    {
        /// <summary>
        /// Calculates the checksum (CRC16) for a 16-bit array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <returns>The CRC16 checksum. [0] = low byte, [1] = high byte.</returns>
        public static byte[] Calculate(this byte[] array)
            => array.Calculate(0, array.Length);

        /// <summary>
        /// Calculates the checksum (CRC16) for a 16-bit array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="start">Starting position.</param>
        /// <param name="length">Length.</param>
        /// <returns>The CRC16 checksum. [0] = low byte, [1] = high byte.</returns>
        public static byte[] Calculate(this byte[] array, int start, int length)
        {
            if (array == null || array.Length == 0)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (start < 0 || start >= array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }

            if (length <= 0 || (start + length) > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            ushort crc16 = 0xFFFF;
            byte lsb;

            for (int i = start; i < (start + length); i++)
            {
                crc16 = (ushort)(crc16 ^ array[i]);
                for (int j = 0; j < 8; j++)
                {
                    lsb = (byte)(crc16 & 1);
                    crc16 = (ushort)(crc16 >> 1);
                    if (lsb == 1)
                    {
                        crc16 = (ushort)(crc16 ^ 0xA001);
                    }
                }
            }

            byte[] b = new byte[2];
            b[0] = (byte)crc16;
            b[1] = (byte)(crc16 >> 8);

            return b;
        }
    }
}
