// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Common.GnssDevice;

namespace Iot.Device.AtModem.Gnss
{
    public class Sim7276Location : Location
    {
        public Sim7276Location(float lat, float lon)
        {
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
