// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp2101
{
    /// <summary>
    /// Watchdog configuration for WDT_CTRL register (0x19), bits [5:4].
    /// </summary>
    public enum WatchdogConfig
    {
        /// <summary>IRQ to pin only.</summary>
        IrqOnly = 0,

        /// <summary>IRQ to pin and reset PMU system.</summary>
        IrqAndReset = 1,

        /// <summary>IRQ to pin, reset PMU system, and pull down PWROK for 1s.</summary>
        IrqResetPullDownPwrok = 2,

        /// <summary>IRQ to pin, reset PMU system, turn off DCDC and LDO, and pull down PWROK.</summary>
        IrqResetAllOff = 3,
    }
}
