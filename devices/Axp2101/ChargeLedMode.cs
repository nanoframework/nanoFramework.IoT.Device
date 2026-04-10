// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp2101
{
    /// <summary>
    /// Charge LED mode settings for CHGLED_SET_CTRL register (0x69).
    /// </summary>
    public enum ChargeLedMode
    {
        /// <summary>LED off.</summary>
        Off = 0,

        /// <summary>LED blink at 1 Hz.</summary>
        Blink1Hz = 1,

        /// <summary>LED blink at 4 Hz.</summary>
        Blink4Hz = 2,

        /// <summary>LED on continuously.</summary>
        On = 3,

        /// <summary>LED controlled by charger state.</summary>
        ControlledByCharger = 4,
    }
}
