// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Max30100
{
    internal static class Max30100DataConverter
    {
        internal static Max30100Reading ConvertFifoSample(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Length != 4)
            {
                throw new ArgumentException("A MAX30100 FIFO sample must contain exactly four bytes.", nameof(data));
            }

            ushort infrared = (ushort)((data[0] << 8) | data[1]);
            ushort red = (ushort)((data[2] << 8) | data[3]);
            return new Max30100Reading(infrared, red);
        }

        internal static double ConvertTemperature(byte integerPart, byte fractionalPart)
        {
            return (sbyte)integerPart + ((fractionalPart & 0x0F) * 0.0625);
        }
    }
}
