// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Amg88xx
{
    internal class TemperatureLowHighBytes
    {
        public TemperatureLowHighBytes(byte low, byte high)
        {
            LowByte = low;
            HighByte = high;
        }

        public byte LowByte { get; set; }

        public byte HighByte { get; set; }
    }
}
