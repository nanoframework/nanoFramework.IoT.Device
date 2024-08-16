// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Common.GnssDevice;

namespace Iot.Device.AtModem.Gnss
{
    /// <summary>
    /// Represents a GNSS device for SIMCom modems.
    /// </summary>
    public abstract class GnssBase : GnssDevice
    {
        /// <summary>
        /// Gets the position of the GNSS device.
        /// </summary>
        /// <returns>A GNSS position or null if none.</returns>
        public abstract Location GetLocation();

        /// <summary>
        /// Gets or sets the interval between wich the GNSS position is updated.
        /// An event is raised when a new valid position is received.
        /// </summary>
        public abstract TimeSpan AutomaticUpdate { get; set; }
    }
}
