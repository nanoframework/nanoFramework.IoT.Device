// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Lsm6Dsl
{
    /// <summary>
    /// Accelerometer or gyroscope output data rate.
    /// </summary>
    public enum OutputDataRate : byte
    {
        /// <summary>The sensor is powered down.</summary>
        PowerDown = 0x00,

        /// <summary>A rate of 12.5 Hz.</summary>
        Rate12Point5Hz = 0x01,

        /// <summary>A rate of 26 Hz.</summary>
        Rate26Hz = 0x02,

        /// <summary>A rate of 52 Hz.</summary>
        Rate52Hz = 0x03,

        /// <summary>A rate of 104 Hz.</summary>
        Rate104Hz = 0x04,

        /// <summary>A rate of 208 Hz.</summary>
        Rate208Hz = 0x05,

        /// <summary>A rate of 416 Hz.</summary>
        Rate416Hz = 0x06,

        /// <summary>A rate of 833 Hz.</summary>
        Rate833Hz = 0x07,

        /// <summary>A rate of 1.66 kHz.</summary>
        Rate1Point66KHz = 0x08,
    }
}