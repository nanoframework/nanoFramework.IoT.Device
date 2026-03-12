// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp2101
{
    /// <summary>
    /// Fast power-on startup sequence level per channel.
    /// Used with FAST_PWRON_SET0-2 registers (0x28-0x2A).
    /// </summary>
    public enum StartSequenceLevel
    {
        /// <summary>Startup sequence level 0 (first to start).</summary>
        Level0 = 0,

        /// <summary>Startup sequence level 1.</summary>
        Level1 = 1,

        /// <summary>Startup sequence level 2 (last to start).</summary>
        Level2 = 2,

        /// <summary>Channel is disabled in the fast power-on sequence.</summary>
        Disable = 3,
    }
}
