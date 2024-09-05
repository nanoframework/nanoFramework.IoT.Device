////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Bq2579x
{
    /// <summary>
    /// Switching frequency for PWM charger.
    /// </summary>
    public enum SwitchingFrequency : byte
    {
        /// <summary>
        /// Frequency 1.5 MHz.
        /// </summary>
        Freq_1_5Mhz = 0b0000_0000,

        /// <summary>
        /// Frequency 750 kHz.
        /// </summary>
        Freq_750_khz = 0b0010_0000,
    }
}
