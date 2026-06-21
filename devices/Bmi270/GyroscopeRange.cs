// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Bmi270
{
    /// <summary>
    /// Gyroscope full-scale range setting.
    /// Bits [2:0] of GYR_RANGE register (0x43).
    /// </summary>
    public enum GyroscopeRange : byte
    {
        /// <summary>Range of plus or minus 2000 degrees per second.</summary>
        Range2000Dps = 0x00,

        /// <summary>Range of plus or minus 1000 degrees per second.</summary>
        Range1000Dps = 0x01,

        /// <summary>Range of plus or minus 500 degrees per second.</summary>
        Range500Dps = 0x02,

        /// <summary>Range of plus or minus 250 degrees per second.</summary>
        Range250Dps = 0x03,

        /// <summary>Range of plus or minus 125 degrees per second.</summary>
        Range125Dps = 0x04,
    }
}
