// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using UnitsNet;

namespace Iot.Device.AtModem.Gnss
{
    /// <summary>
    /// Represents a Global Navigation Satellite System position.
    /// </summary>
    public class GnssPosition
    {
        /// <summary>
        /// Represents the fix mode of the GNSS.
        /// </summary>
        public enum FixMode
        {
            /// <summary>
            /// 2D fix.
            /// </summary>
            Fix2d = 2,

            /// <summary>
            /// 3D fix.
            /// </summary>
            Fix3d = 3,
        }

        /// <summary>
        /// Gets or sets the date and time of the GNSS position.
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Gets or sets the mode of the GNSS position.
        /// </summary>
        public FixMode Mode { get; set; }

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

        /// <summary>
        /// Gets or sets the latitude of the GNSS position.
        /// </summary>
        public float Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude of the GNSS position.
        /// </summary>
        public float Longitude { get; set; }

        /// <summary>
        /// Gets or sets the altitude of the GNSS position.
        /// </summary>
        public float Altitude { get; set; }

        /// <summary>
        /// Gets or sets the speed of the GNSS position.
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// Gets or sets the course angle of the GNSS position.
        /// </summary>
        public Angle Course { get; set; }

        /// <summary>
        /// Gets or sets the Position Dilution of Precision of the GNSS position.
        /// </summary>
        public float PositionDilutionOfPrecision { get; set; }

        /// <summary>
        /// Gets or sets the Horizontal Dilution of Precision of the GNSS position.
        /// </summary>
        public float HorizontalDilutionOfPrecision { get; set; }

        /// <summary>
        /// Gets or sets the Vertical Dilution of Precision of the GNSS position.
        /// </summary>
        public float VerticalDilutionOfPrecision { get; set; }
    }
}