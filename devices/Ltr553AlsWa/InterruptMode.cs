// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ltr553AlsWa
{
    /// <summary>
    /// Interrupt mode setting for the LTR-553ALS-WA.
    /// </summary>
    public enum InterruptMode : byte
    {
        /// <summary>Interrupt pin inactive (default).</summary>
        Inactive = 0x00,

        /// <summary>Only proximity sensor triggers interrupt.</summary>
        PsOnly = 0x01,

        /// <summary>Only ambient light sensor triggers interrupt.</summary>
        AlsOnly = 0x02,

        /// <summary>Both PS and ALS can trigger interrupt.</summary>
        Both = 0x03,
    }
}
