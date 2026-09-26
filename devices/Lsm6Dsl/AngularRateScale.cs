// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Lsm6Dsl
{
    /// <summary>
    /// Gyroscope full-scale range.
    /// </summary>
    public enum AngularRateScale : byte
    {
        /// <summary>Plus or minus 250 degrees per second.</summary>
        Scale0250Dps = 0b0000,

        /// <summary>Plus or minus 125 degrees per second.</summary>
        Scale0125Dps = 0b0010,

        /// <summary>Plus or minus 500 degrees per second.</summary>
        Scale0500Dps = 0b0100,

        /// <summary>Plus or minus 1000 degrees per second.</summary>
        Scale1000Dps = 0b1000,

        /// <summary>Plus or minus 2000 degrees per second.</summary>
        Scale2000Dps = 0b1100,
    }
}