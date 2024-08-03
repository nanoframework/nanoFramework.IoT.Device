// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Common.GnssDevice
{
    /// <summary>
    /// Defines the Gnss module mode.
    /// </summary>
    public enum GnssOperation : byte
    {
        /// <summary>
        /// Represents an unknown mode of the Gnss module.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Represents an automatic mode of operation for the Gnss module.
        /// </summary>
        Auto = 1,

        /// <summary>
        /// Represents a manual mode of the Gnss module.
        /// </summary>
        Manual = 2
    }
}