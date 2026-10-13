// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Apds9960
{
    /// <summary>
    /// APDS9960 infrared LED drive strength.
    /// </summary>
    public enum LedDriveStrength : byte
    {
        /// <summary>A 100 mA drive.</summary>
        MilliAmps100 = 0,

        /// <summary>A 50 mA drive.</summary>
        MilliAmps50 = 1,

        /// <summary>A 25 mA drive.</summary>
        MilliAmps25 = 2,

        /// <summary>A 12.5 mA drive.</summary>
        MilliAmps12_5 = 3,
    }
}
