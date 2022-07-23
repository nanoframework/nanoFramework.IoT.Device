// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Rtc
{
    /// <summary>
    /// Helper that allows setting and getting each bit separately
    /// </summary>
    internal static class BitHelper
    {
        public static byte ClearBit(this byte data, int bitNumber) => (byte)(data & ~(1 << bitNumber));

        public static bool IsBitSet(this byte data, int bitNumber) => (data & (1 << bitNumber)) != 0;

        public static byte SetBit(byte data, int bitNumber) => (byte)(data | (1 << bitNumber));

        public static bool GetBit(this byte b, int bitNumber) =>  (b & (1 << bitNumber - 1)) != 0;

    }
}