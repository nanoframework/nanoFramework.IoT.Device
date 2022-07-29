// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Axp192
{
    /// <summary>
    /// Gpio 1 and 2 behavior.
    /// </summary>
    public enum Gpio12Behavior
    {
        /// <summary>NMOS Leak Open Output.</summary>
        MnosLeakOpenOutput = 0,

        /// <summary>Universal Input Function.</summary>
        UniversalInputFunction = 1,

        /// <summary>Low Noise LDO.</summary>
        PwmOutput = 2,

        /// <summary>ADC Input.</summary>
        AdcInput = 4,

        /// <summary>Low Output.</summary>
        LowOutput = 5,

        /// <summary>Floating behavior.</summary>
        Floating = 5,
    }
}
