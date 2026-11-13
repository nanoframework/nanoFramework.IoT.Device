// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Atecc608
{
    /// <summary>
    /// ATECC608 memory zones.
    /// </summary>
    public enum Atecc608Zone : byte
    {
        /// <summary>Configuration zone (128 bytes).</summary>
        Config = 0x00,

        /// <summary>OTP (One-Time Programmable) zone (64 bytes).</summary>
        Otp = 0x01,

        /// <summary>Data zone (slots 0-15).</summary>
        Data = 0x02,
    }
}
