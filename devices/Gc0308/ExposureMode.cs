// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Gc0308
{
    /// <summary>
    /// Exposure control mode for the GC0308 camera sensor.
    /// </summary>
    public enum ExposureMode : byte
    {
        /// <summary>Automatic exposure control (AEC). The sensor adjusts exposure automatically.</summary>
        Auto = 0x01,

        /// <summary>Manual exposure control. Exposure must be set via registers.</summary>
        Manual = 0x00,
    }
}
