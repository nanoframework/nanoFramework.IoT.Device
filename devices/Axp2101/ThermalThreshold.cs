// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp2101
{
    /// <summary>
    /// Die over-temperature threshold settings for THE_REGU_THRES_SET register (0x65), bits [1:0].
    /// </summary>
    public enum ThermalThreshold
    {
        /// <summary>Thermal regulation at 60°C.</summary>
        Temperature60C = 0,

        /// <summary>Thermal regulation at 80°C.</summary>
        Temperature80C = 1,

        /// <summary>Thermal regulation at 100°C.</summary>
        Temperature100C = 2,

        /// <summary>Thermal regulation at 120°C.</summary>
        Temperature120C = 3,
    }
}
