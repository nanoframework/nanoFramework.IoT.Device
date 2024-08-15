// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Common.GnssDevice;

namespace Iot.Device.AtModem.Gnss
{
    /// <summary>
    /// Location information from a SIM7276 GNSS module.
    /// </summary>
    public class Sim7672Location : Location
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sim7672Location"/> class.
        /// </summary>
        /// <param name="lat">The latitude.</param>
        /// <param name="lon">The longitude.</param>
        public Sim7672Location(float lat, float lon)
        {
            Latitude = lat;
            Longitude = lon;
        }

        /// <summary>
        /// Gets or sets the number of visible GPS satellites.
        /// </summary>
        public int GpsNumberVisibleSatellites { get; set; }

        /// <summary>
        /// Gets or sets the number of visible GLONASS satellites.
        /// </summary>
        public int GlonassNumberVisibleSatellites { get; set; }

        /// <summary>
        /// Gets or sets the number of visible Beidou satellites.
        /// </summary>
        public int BeidouNumberVisibleSatellites { get; set; }

        /// <summary>
        /// Gets or sets the number of visible Galileo satellites.
        /// </summary>
        public int GalileoNumberVisibleSatellites { get; set; }

        /// <summary>
        /// Gets or sets the total number of satellites used.
        /// </summary>
        public int TotalNumberOfSatellitesUsed { get; set; }
    }
}
