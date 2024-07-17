// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Atgm336h
{
    /// <summary>
    /// Defines the GPS module fix status.
    /// </summary>
    public enum Fix : byte
    {
        /// <summary> Represents no fix mode of the GPS module.
        /// </summary>
        NoFix = 1,

        /// <summary>
        /// Represents a 2D fix status of the GPS module.
        /// </summary>
        Fix2D = 2,

        /// <summary>
        /// Represents a 3D fix status of the GPS module.
        /// </summary>
        Fix3D = 3
    }
}