// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Bmi270
{
    /// <summary>
    /// Gyroscope output data rate (ODR) setting.
    /// Bits [3:0] of GYR_CONF register (0x42).
    /// </summary>
    public enum GyroscopeOutputDataRate : byte
    {
        /// <summary>Output data rate of 25 Hz.</summary>
        Odr25Hz = 0x06,

        /// <summary>Output data rate of 50 Hz.</summary>
        Odr50Hz = 0x07,

        /// <summary>Output data rate of 100 Hz.</summary>
        Odr100Hz = 0x08,

        /// <summary>Output data rate of 200 Hz (default).</summary>
        Odr200Hz = 0x09,

        /// <summary>Output data rate of 400 Hz.</summary>
        Odr400Hz = 0x0A,

        /// <summary>Output data rate of 800 Hz.</summary>
        Odr800Hz = 0x0B,

        /// <summary>Output data rate of 1600 Hz.</summary>
        Odr1600Hz = 0x0C,

        /// <summary>Output data rate of 3200 Hz.</summary>
        Odr3200Hz = 0x0D,
    }
}
