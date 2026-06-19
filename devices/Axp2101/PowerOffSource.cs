// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Axp2101
{
    /// <summary>
    /// Power-off source flags from PWROFF_STATUS register (0x21).
    /// Multiple sources can be active simultaneously.
    /// </summary>
    [Flags]
    public enum PowerOffSource
    {
        /// <summary>POWERON pull-down for off level when POWERON Mode as POWEROFF Source.</summary>
        PowerKeyPullDown = 0b0000_0001,

        /// <summary>Software configuration as POWEROFF Source.</summary>
        SoftwareOff = 0b0000_0010,

        /// <summary>POWERON always low when EN Mode as POWEROFF Source.</summary>
        PowerOnLow = 0b0000_0100,

        /// <summary>Vsys under voltage as POWEROFF Source.</summary>
        VsysUnderVoltage = 0b0000_1000,

        /// <summary>VBUS over voltage as POWEROFF Source.</summary>
        VbusOverVoltage = 0b0001_0000,

        /// <summary>DCDC under voltage as POWEROFF Source.</summary>
        DcDcUnderVoltage = 0b0010_0000,

        /// <summary>DCDC over voltage as POWEROFF Source.</summary>
        DcDcOverVoltage = 0b0100_0000,

        /// <summary>Die over temperature as POWEROFF Source.</summary>
        OverTemperature = 0b1000_0000,
    }
}
