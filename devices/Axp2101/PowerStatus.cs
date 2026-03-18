// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Axp2101
{
    /// <summary>
    /// Power status flags from Status1 register (0x00).
    /// </summary>
    [Flags]
    public enum PowerStatus
    {
        /// <summary>VBUS voltage present and good.</summary>
        VbusGood = 0b0010_0000,

        /// <summary>BATFET present state (battery output path active).</summary>
        BatfetPresent = 0b0001_0000,

        /// <summary>Battery detected and connected.</summary>
        BatteryPresent = 0b0000_1000,

        /// <summary>Battery is in activation mode.</summary>
        BatteryActivationMode = 0b0000_0100,

        /// <summary>Thermal regulation is active.</summary>
        ThermalRegulation = 0b0000_0010,

        /// <summary>Current limit is active on input.</summary>
        CurrentLimit = 0b0000_0001,
    }
}
