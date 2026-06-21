// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Bmi270
{
    /// <summary>
    /// Accelerometer output data rate (ODR) setting.
    /// Bits [3:0] of ACC_CONF register (0x40).
    /// </summary>
    public enum AccelerometerOutputDataRate : byte
    {
        /// <summary>Output data rate of 0.78 Hz.</summary>
        Odr0_78Hz = 0x01,

        /// <summary>Output data rate of 1.5625 Hz.</summary>
        Odr1_5625Hz = 0x02,

        /// <summary>Output data rate of 3.125 Hz.</summary>
        Odr3_125Hz = 0x03,

        /// <summary>Output data rate of 6.25 Hz.</summary>
        Odr6_25Hz = 0x04,

        /// <summary>Output data rate of 12.5 Hz.</summary>
        Odr12_5Hz = 0x05,

        /// <summary>Output data rate of 25 Hz.</summary>
        Odr25Hz = 0x06,

        /// <summary>Output data rate of 50 Hz.</summary>
        Odr50Hz = 0x07,

        /// <summary>Output data rate of 100 Hz (default).</summary>
        Odr100Hz = 0x08,

        /// <summary>Output data rate of 200 Hz.</summary>
        Odr200Hz = 0x09,

        /// <summary>Output data rate of 400 Hz.</summary>
        Odr400Hz = 0x0A,

        /// <summary>Output data rate of 800 Hz.</summary>
        Odr800Hz = 0x0B,

        /// <summary>Output data rate of 1600 Hz.</summary>
        Odr1600Hz = 0x0C,
    }
}
