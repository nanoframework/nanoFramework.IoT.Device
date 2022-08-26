// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp7940xx
{
    /// <summary>
    /// Defines the oscillator source configuration for the clock.
    /// </summary>
    public enum ClockSource
    {
        /// <summary>
        /// A 32.768 kHz crystal oscillator is connected to the X1 and X2 pins.
        /// </summary>
        ExternalCrystal,

        /// <summary>
        /// A 32.768 kHz external clock source is connected to the X1 pin.
        /// </summary>
        /// <remarks>
        /// When using this configuration, the X2 pin must be left floating.
        /// </remarks>
        ExternalClockInput
    }
}
