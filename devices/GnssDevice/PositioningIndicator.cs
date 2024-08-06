// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Common.GnssDevice
{
    /// <summary>
    /// NMEA Positioning System Mode Indicator.
    /// </summary>
    public enum PositioningIndicator
    {
        /// <summary>A for Autonomous.</summary>
        Autonomous,

        /// <summary>D for Differential.</summary>
        Differential,

        /// <summary>E for Estimated (dead reckoning) mode.</summary>
        Estimated,

        /// <summary>M for Manual input.</summary>
        Manual,

        /// <summary>N for Data not valid.</summary>
        NotValid,
    }
}
