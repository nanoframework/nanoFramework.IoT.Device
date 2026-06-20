// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp2101
{
    /// <summary>
    /// IRQ/POWERON press timing for IRQ_OFF_ON_LEVEL_CTRL register (0x27), bits [5:4].
    /// </summary>
    public enum IrqTime
    {
        /// <summary>IRQ level time 1 second.</summary>
        Time1s = 0,

        /// <summary>IRQ level time 1.5 seconds.</summary>
        Time1s5 = 1,

        /// <summary>IRQ level time 2 seconds.</summary>
        Time2s = 2,

        /// <summary>IRQ level time 2.5 seconds.</summary>
        Time2s5 = 3,
    }
}
