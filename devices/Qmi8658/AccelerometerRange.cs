// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Qmi8658
{
    /// <summary>
    /// Accelerometer full-scale ranges encoded in CTRL2 bits 6:4.
    /// </summary>
    public enum AccelerometerRange : byte
    {
        /// <summary>Range of plus or minus 2 g.</summary>
        Range2G = 0x00,

        /// <summary>Range of plus or minus 4 g.</summary>
        Range4G = 0x01,

        /// <summary>Range of plus or minus 8 g.</summary>
        Range8G = 0x02,

        /// <summary>Range of plus or minus 16 g.</summary>
        Range16G = 0x03,
    }
}
