// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp2101
{
    /// <summary>
    /// PWROK delay settings for PWROK_SEQU_CTRL register (0x25), bits [1:0].
    /// </summary>
    public enum PwrokDelay
    {
        /// <summary>PWROK delay 8 ms.</summary>
        Delay8ms = 0,

        /// <summary>PWROK delay 16 ms.</summary>
        Delay16ms = 1,

        /// <summary>PWROK delay 32 ms.</summary>
        Delay32ms = 2,

        /// <summary>PWROK delay 64 ms.</summary>
        Delay64ms = 3,
    }
}
