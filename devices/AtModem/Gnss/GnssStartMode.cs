// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.AtModem.Gnss
{
    /// <summary>
    /// Represents the start mode of a GNSS device.
    /// </summary>
    public enum GnssStartMode
    {
        /// <summary>
        /// Represents a cold start mode.
        /// </summary>
        Cold = 0,

        /// <summary>
        /// Represents a warm start mode.
        /// </summary>
        Warm,

        /// <summary>
        /// Represents a hot start mode.
        /// </summary>
        Hot,
    }
}
