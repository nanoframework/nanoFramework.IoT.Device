// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Hdc1080
{
    /// <summary>
    /// Helper that allows setting and getting each bit separately
    /// </summary>
    internal static class BitHelper
    {
        public static byte SetBit(this byte b, int pos, bool value)
        {
            if (value)
            {
                return (byte)(b | (1 << pos));
            }
            return (byte)(b & ~(1 << pos));
        }

        public static bool GetBit(this byte b, int bitNumber)
        {
            return (b & (1 << bitNumber - 1)) != 0;
        }
    }
}