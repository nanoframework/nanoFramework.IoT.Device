// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ltr553AlsWa
{
    /// <summary>
    /// Interrupt polarity setting for the LTR-553ALS-WA.
    /// </summary>
    public enum InterruptPolarity : byte
    {
        /// <summary>Active low (default). INT pin requires pull-up resistor.</summary>
        ActiveLow = 0x00,

        /// <summary>Active high. INT pin requires pull-down resistor.</summary>
        ActiveHigh = 0x01,
    }
}
