// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
using System;

namespace Iot.Device.Sen5x.Extensions
{
    internal static class SpanByteExtensions
    {
        /// <summary>
        /// A convenient method that verifies all pairs within a data span. The data must be a multiple of 3 bytes (2 bytes data, 1 byte crc).
        /// </summary>
        /// <param name="data">The data to be verified.</param>
        /// <exception cref="ArgumentException">When the byte lenght is not a multiple of 3.</exception>
        /// <exception cref="IndexOutOfRangeException">When the CRC fails.</exception>
        internal static void VerifyCrc(this SpanByte data)
        {
            if (data.Length % 3 != 0)
            {
                throw new ArgumentException();
            }

            for (int i = 0; i < data.Length; i += 3)
            {
                var chk = data.Slice(i, 2).CalculateCrc();
                if (data[i + 2] != chk)
                {
                    throw new IndexOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// A convenient method that calculates the CRC for all pairs within the data span. The space for the CRC is assumed to be reserved and this method fills those in.
        /// </summary>
        /// <param name="data">The span in which the CRC must be calculated per pair.</param>
        /// <exception cref="ArgumentException">When the number of bytes is not a multiple of 3.</exception>
        internal static void UpdateCrc(this SpanByte data)
        {
            if (data.Length % 3 != 0)
            {
                throw new ArgumentException();
            }

            for (int i = 0; i < data.Length; i += 3)
            {
                data[i + 2] = data.Slice(i, 2).CalculateCrc();
            }
        }

        /// <summary>
        /// Calculates the CRC as provided in the datasheet, slightly adapted for nanoFramework.
        /// </summary>
        /// <param name="data">The data to be CRC'ed. For SEN5x this should only be pairs of 2 bytes.</param>
        /// <returns>The calculated CRC value.</returns>
        private static byte CalculateCrc(this SpanByte data)
        {
            byte crc = 0xFF;
            for (int i = 0; i < data.Length; i++)
            {
                crc ^= data[i];
                for (byte bit = 8; bit > 0; bit--)
                {
                    if ((crc & 0x80) == 0x80)
                    {
                        crc = (byte)(crc << 1 ^ 0x31u);
                    }
                    else
                    {
                        crc = (byte)(crc << 1);
                    }
                }
            }

            return crc;
        }
    }
}
