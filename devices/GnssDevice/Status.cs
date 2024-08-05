// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Common.GnssDevice
{
    /// <summary>
    /// NMEA data status.
    /// </summary>
    public enum Status
    {
        /// <summary>A for Valid data.</summary>
        Valid,

        /// <summary>V for invalid data.</summary>
        NotValid,
    }
}
