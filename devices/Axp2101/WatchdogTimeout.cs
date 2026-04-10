// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp2101
{
    /// <summary>
    /// Watchdog timeout period for WDT_CTRL register (0x19), bits [2:0].
    /// </summary>
    public enum WatchdogTimeout
    {
        /// <summary>Watchdog timeout 1 second.</summary>
        Timeout1s = 0,

        /// <summary>Watchdog timeout 2 seconds.</summary>
        Timeout2s = 1,

        /// <summary>Watchdog timeout 4 seconds.</summary>
        Timeout4s = 2,

        /// <summary>Watchdog timeout 8 seconds.</summary>
        Timeout8s = 3,

        /// <summary>Watchdog timeout 16 seconds.</summary>
        Timeout16s = 4,

        /// <summary>Watchdog timeout 32 seconds.</summary>
        Timeout32s = 5,

        /// <summary>Watchdog timeout 64 seconds.</summary>
        Timeout64s = 6,

        /// <summary>Watchdog timeout 128 seconds.</summary>
        Timeout128s = 7,
    }
}
