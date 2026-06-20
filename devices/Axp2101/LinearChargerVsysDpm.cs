// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp2101
{
    /// <summary>
    /// Linear charger Vsys DPM voltage settings for MIN_SYS_VOL_CTRL register (0x14), bits [6:4].
    /// </summary>
    public enum LinearChargerVsysDpm
    {
        /// <summary>Vsys DPM voltage 4.1V.</summary>
        Voltage4V1 = 0,

        /// <summary>Vsys DPM voltage 4.2V.</summary>
        Voltage4V2 = 1,

        /// <summary>Vsys DPM voltage 4.3V.</summary>
        Voltage4V3 = 2,

        /// <summary>Vsys DPM voltage 4.4V.</summary>
        Voltage4V4 = 3,

        /// <summary>Vsys DPM voltage 4.5V.</summary>
        Voltage4V5 = 4,

        /// <summary>Vsys DPM voltage 4.6V.</summary>
        Voltage4V6 = 5,

        /// <summary>Vsys DPM voltage 4.7V.</summary>
        Voltage4V7 = 6,

        /// <summary>Vsys DPM voltage 4.8V.</summary>
        Voltage4V8 = 7,
    }
}
