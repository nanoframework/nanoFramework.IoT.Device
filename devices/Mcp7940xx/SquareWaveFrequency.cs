// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp7940xx
{
    /// <summary>
    /// Sets the frequency of the square wave output on the MFP pin.
    /// </summary>
    public enum SquareWaveFrequency
    {
        /// <summary>
        /// Square wave with 1 Hz frequency.
        /// </summary>
        Frequency1Hz = 0b00,

        /// <summary>
        /// Square wave with 4.096 kHz frequency.
        /// </summary>
        Frequency4kHz = 0b01,

        /// <summary>
        /// Square wave with 8.192 kHz frequency.
        /// </summary>
        Frequency8kHz = 0b10,

        /// <summary>
        /// Square wave with 32.768 kHz frequency.
        /// </summary>
        Frequency32kHz = 0b11
    }
}
