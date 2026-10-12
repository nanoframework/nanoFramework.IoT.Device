// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Max30100
{
    internal static class Max30100DataConverter
    {
        private const byte FifoPointerMask = 0x0F;
        private const byte FifoDepth = 16;
        private const int FifoSampleLength = 4;
        private const int ByteBitCount = 8;
        private const byte TemperatureFractionMask = 0x0F;
        private const double TemperatureFractionResolution = 0.0625;

        /// <summary>Calculates the number of unread FIFO samples.</summary>
        /// <param name="writePointer">The FIFO write pointer.</param>
        /// <param name="readPointer">The FIFO read pointer.</param>
        /// <param name="overflowCount">The FIFO overflow count.</param>
        /// <returns>The number of unread samples.</returns>
        internal static byte CalculateUnreadSampleCount(byte writePointer, byte readPointer, byte overflowCount)
        {
            byte count = (byte)((writePointer - readPointer) & FifoPointerMask);
            return count == 0 && overflowCount != 0 ? FifoDepth : count;
        }

        /// <summary>Converts a four-byte FIFO payload into an optical reading.</summary>
        /// <param name="data">The four-byte FIFO payload.</param>
        /// <returns>The converted optical reading.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="data"/> does not contain exactly four bytes.</exception>
        internal static Max30100Reading ConvertFifoSample(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException();
            }

            if (data.Length != FifoSampleLength)
            {
                throw new ArgumentException();
            }

            ushort infrared = (ushort)((data[0] << ByteBitCount) | data[1]);
            ushort red = (ushort)((data[2] << ByteBitCount) | data[3]);
            return new Max30100Reading(infrared, red);
        }

        /// <summary>Converts the raw integer and fractional temperature registers.</summary>
        /// <param name="integerPart">The signed integer temperature register.</param>
        /// <param name="fractionalPart">The fractional temperature register.</param>
        /// <returns>The temperature in degrees Celsius.</returns>
        internal static double ConvertTemperature(byte integerPart, byte fractionalPart)
        {
            return (sbyte)integerPart
                + ((fractionalPart & TemperatureFractionMask) * TemperatureFractionResolution);
        }
    }
}
