// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Hx711
{
    /// <summary>
    /// Defines the available gain factors available.
    /// </summary>
    public enum GainLevel : byte
    {
        // developer note:
        // the enum value corresponds to the number of CLK cycles that have to be sent to device to configure the gain factor
        // (this is coming from the device datasheet)

        /// <summary>
        /// Gain factor not set.
        /// </summary>
        None = 0,

        /// <summary>
        /// Gain factor of 32. Channel B.
        /// </summary>
        GainB32 = 0b1010_0000,

        /// <summary>
        /// Gain factor of 64. Channel A.
        /// </summary>
        GainA64 = 0b1010_1000,

        /// <summary>
        /// Gain factor of 128. Channel A.
        /// </summary>
        GainA128 = 0b1000_0000
    }
}
