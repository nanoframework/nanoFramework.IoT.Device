// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Bmxx80
{
    /// <summary>
    /// Controls the inactive duration in normal mode.
    /// </summary>
    public enum StandbyTime : byte
    {
        /// <summary>
        /// Standby time 0.5 ms.
        /// </summary>
        Ms0_5 = 0b000,

        /// <summary>
        /// Standby time 62.5 ms.
        /// </summary>
        Ms62_5 = 0b001,

        /// <summary>
        /// Standby time 125 ms.
        /// </summary>
        Ms125 = 0b010,

        /// <summary>
        /// Standby time 250 ms.
        /// </summary>
        Ms250 = 0b011,

        /// <summary>
        /// Standby time 500 ms.
        /// </summary>
        Ms500 = 0b100,

        /// <summary>
        /// Standby time 1000 ms.
        /// </summary>
        Ms1000 = 0b101,

        /// <summary>
        /// Standby time 10 ms.
        /// </summary>
        Ms10 = 0b110,

        /// <summary>
        /// Standby time 20 ms.
        /// </summary>
        Ms20 = 0b111,
    }
}
