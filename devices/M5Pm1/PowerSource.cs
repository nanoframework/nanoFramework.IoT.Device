// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.M5Pm1
{
    /// <summary>
    /// Identifies the active power source reported by the M5PM1.
    /// </summary>
    public enum PowerSource
    {
        /// <summary>
        /// Powered from the 5V input (USB-C in).
        /// </summary>
        FiveVoltInput = 0,

        /// <summary>
        /// Powered from the 5V input/output path.
        /// </summary>
        FiveVoltInputOutput = 1,

        /// <summary>
        /// Powered from the battery.
        /// </summary>
        Battery = 2,

        /// <summary>
        /// The power source is unknown (the register reported a value outside the documented range).
        /// </summary>
        Unknown = 3,
    }
}
