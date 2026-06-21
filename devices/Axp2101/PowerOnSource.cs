// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Axp2101
{
    /// <summary>
    /// Power-on source flags from PWRON_STATUS register (0x20).
    /// Multiple sources can be active simultaneously.
    /// </summary>
    [Flags]
    public enum PowerOnSource
    {
        /// <summary>POWERON low for on level when POWERON Mode as POWERON Source.</summary>
        PowerOnLow = 0b0000_0001,

        /// <summary>IRQ PIN pull-down as POWERON Source.</summary>
        IrqPinLow = 0b0000_0010,

        /// <summary>VBUS insert and good as POWERON Source.</summary>
        VbusInsert = 0b0000_0100,

        /// <summary>Battery voltage greater than 3.3V when charged as POWERON Source.</summary>
        BatteryCharge = 0b0000_1000,

        /// <summary>Battery insert and good as POWERON Source.</summary>
        BatteryInsert = 0b0001_0000,

        /// <summary>POWERON always high when EN Mode as POWERON Source.</summary>
        EnMode = 0b0010_0000,
    }
}
