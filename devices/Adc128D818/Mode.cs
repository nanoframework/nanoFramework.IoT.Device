// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Adc128D818
{
    /// <summary>
    /// ADC128D818 operating mode.
    /// </summary>
    public enum Mode : byte
    {
        // Details in Table 2. Serial Bus Address Table

        /// <summary>
        /// Mode 0: Single-Ended input mode. All channels are converted, except. Includes the local temperature.
        /// </summary>
        Mode0 = 0b0000_0000,

        /// <summary>
        /// Mode 1: Single-Ended input mode. All channels are converted. Local temperature is excluded.
        /// </summary>
        Mode1 = 0b0000_0010,

        /// <summary>
        /// Mode 2: Pseudo-Differential input mode. Channels are converted in pairs. Includes the local temperature.
        /// </summary>
        Mode2 = 0b0000_0100,

        /// <summary>
        /// Mode 3: Mixed Single-Ended and Pseudo-Differential input mode. Channels 0-3 are converted as single-ended inputs, channels 4-7 are converted as pseudo-differential pairs. Includes the local temperature.
        /// </summary>
        Mode3 = 0b0000_0110
    }
}
