// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Bmi270
{
    /// <summary>
    /// Accelerometer full-scale range setting.
    /// Bits [1:0] of ACC_RANGE register (0x41).
    /// </summary>
    public enum AccelerometerRange : byte
    {
        /// <summary>Range of plus or minus 2g.</summary>
        Range2G = 0x00,

        /// <summary>Range of plus or minus 4g.</summary>
        Range4G = 0x01,

        /// <summary>Range of plus or minus 8g.</summary>
        Range8G = 0x02,

        /// <summary>Range of plus or minus 16g.</summary>
        Range16G = 0x03,
    }
}
