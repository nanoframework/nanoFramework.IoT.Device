// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.M5Pm1
{
    /// <summary>
    /// Multiplexed function of an M5PM1 GPIO pin.
    /// </summary>
    public enum GpioFunction
    {
        /// <summary>
        /// General-purpose I/O.
        /// </summary>
        Gpio = 0,

        /// <summary>
        /// Interrupt request source.
        /// </summary>
        Irq = 1,

        /// <summary>
        /// Wake-up source.
        /// </summary>
        Wake = 2,

        /// <summary>
        /// Pin-specific special function (ADC, PWM or LED, depending on the pin).
        /// </summary>
        Special = 3,
    }
}
