// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Rtc
{
    /// <summary>
    /// Register of Ds1302.
    /// </summary>
    internal enum Ds1302Registers : byte
    {
        REG_SECONDS = 0x80,
        REG_WP = 0x8E,
        REG_BURST = 0xBE,
    }
}