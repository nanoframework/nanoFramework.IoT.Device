// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Common.GnssDevice
{
    /// <summary>
    /// Defines the Gnss module fix status.
    /// </summary>
    public enum Fix : byte
    {
        /// <summary> Represents no fix mode of the Gnss module.
        /// </summary>
        NoFix = 1,

        /// <summary>
        /// Represents a 2D fix status of the Gnss module.
        /// </summary>
        Fix2D = 2,

        /// <summary>
        /// Represents a 3D fix status of the Gnss module.
        /// </summary>
        Fix3D = 3
    }
}