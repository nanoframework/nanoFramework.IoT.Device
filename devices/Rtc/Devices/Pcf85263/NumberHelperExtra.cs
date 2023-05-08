// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Rtc
{
    /// <summary>
    /// Helper that converts numbers.
    /// </summary>
    public static class NumberHelperExtra
    {
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
    }
}
