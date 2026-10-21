// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Qmi8658
{
    /// <summary>
    /// Gyroscope full-scale ranges encoded in CTRL3 bits 6:4.
    /// </summary>
    public enum GyroscopeRange : byte
    {
        /// <summary>Range of plus or minus 16 degrees per second.</summary>
        Range16Dps = 0x00,

        /// <summary>Range of plus or minus 32 degrees per second.</summary>
        Range32Dps = 0x01,

        /// <summary>Range of plus or minus 64 degrees per second.</summary>
        Range64Dps = 0x02,

        /// <summary>Range of plus or minus 128 degrees per second.</summary>
        Range128Dps = 0x03,

        /// <summary>Range of plus or minus 256 degrees per second.</summary>
        Range256Dps = 0x04,

        /// <summary>Range of plus or minus 512 degrees per second.</summary>
        Range512Dps = 0x05,

        /// <summary>Range of plus or minus 1024 degrees per second.</summary>
        Range1024Dps = 0x06,

        /// <summary>Range of plus or minus 2048 degrees per second.</summary>
        Range2048Dps = 0x07,
    }
}
