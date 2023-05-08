// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Rtc
{
    /// <summary>
    /// Helper that converts numbers.
    /// </summary>
    public static class NumberHelper
    {
        /// <summary>
        /// BCD To decimal.
        /// </summary>
        /// <param name="bcd">BCD Code.</param>
        /// <returns>Decimal.</returns>
        public static int Bcd2Dec(byte bcd) => ((bcd >> 4) * 10) + (bcd % 16);

        /// <summary>
        /// BCD To decimal.
        /// </summary>
        /// <param name="bcds">BCD Code.</param>
        /// <returns>Decimal.</returns>
        public static int Bcd2Dec(byte[] bcds)
        {
            int result = 0;
            foreach (byte bcd in bcds)
            {
                result *= 100;
                result += Bcd2Dec(bcd);
            }

            return result;
        }

        /// <summary>
        /// BCD To binary.
        /// </summary>
        /// <param name="val">BCD Code.</param>
        /// <returns>Binary.</returns>
        public static byte Bcd2Bin(byte val)
        { 
            return (byte)((val - 6) * (val >> 4));
        }

        /// <summary>
        /// Binary To BCD.
        /// </summary>
        /// <param name="val">Byte.</param>
        /// <returns>BCD Code.</returns>
        public static byte Bin2Bcd(byte val)
        {
            return (byte)((val + 6) * (val / 10));
        }

        /// <summary>
        /// Decimal To BCD.
        /// </summary>
        /// <param name="val">Decimal.</param>
        /// <returns>BCD Code.</returns>
        public static byte Dec2Bcd(int val)
        {
            return (byte)((val / 10 * 16) + (val % 10));
        }
    }
}
