// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Common.GpsDevice
{
    /// <summary>
    /// Represents the GNGLL NMEA0183 data from a GPS device.
    /// </summary>
    public class GngllData
    {
        /// <summary>
        /// Gets the location information in Geographic Position System (GPS) coordinates.
        /// </summary>
        public GeoPosition Location { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GngllData" /> class.
        /// </summary>
        /// <param name="location">Location information.</param>
        public GngllData(GeoPosition location)
        {
            Location = location;
        }
    }
}