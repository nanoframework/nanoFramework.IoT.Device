﻿using System;

namespace Iot.Device.Axp192
{
    /// <summary>
    /// GPIO3 behavior
    /// </summary>
    public enum Gpio3Behavior
    {
        /// <summary>External charging</summary>
        ExternalCharing = 0,

        /// <summary>NMOS Leak Open Output</summary>
        MnosLeakOpenOutput = 1,

        /// <summary>Universal Input Function</summary>
        UniversalInputFunction = 2,

        /// <summary>ADC Input</summary>
        AdcInput = 3,
    }
}
