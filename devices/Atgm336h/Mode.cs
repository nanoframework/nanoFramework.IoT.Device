// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Atgm336h
{
    /// <summary>
    /// Defines the GPS module mode.
    /// </summary>
    public enum Mode : byte
    {
        /// <summary>
        /// Represents an unknown mode of the GPS module.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Represents an automatic mode of operation for the GPS module.
        /// </summary>
        Auto = 1,

        /// <summary>
        /// Represents a manual mode of the GPS module.
        /// </summary>
        Manual = 2
    }
}