// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.AtomQrCode
{
    /// <summary>
    /// Options for controlling the reader positioning light.
    /// </summary>
    public enum Positioninglight
    {
        /// <summary>
        /// Positioning light on when reading.
        /// </summary>
        OnWhenReading,

        /// <summary>
        /// Positioning light always on.
        /// </summary>
        AlwaysOn,

        /// <summary>
        /// Positioning light always off.
        /// </summary>
        AlwaysOff
    }
}
