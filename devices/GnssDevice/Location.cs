// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using UnitsNet;

namespace Iot.Device.Common.GnssDevice
{
    /// <summary>
    /// Represents a geographic position with latitude and longitude coordinates.
    /// </summary>
    public class Location
    {
        /// <summary>
        /// Gets the latitude of a geographic position.
        /// </summary>
        public double Latitude { get; internal set; }

        /// <summary>
        /// Gets the longitude of a geographic position.
        /// </summary>
        public double Longitude { get; internal set; }

        /// <summary>
        /// Gets or sets the altitude of the GNSS position.
        /// </summary>
        public double Altitude { get; set; }

        /// <summary>
        /// Gets or sets the speed of the GNSS position.
        /// </summary>
        public Speed Speed { get; set; }

        /// <summary>
        /// Gets or sets the course angle of the GNSS position.
        /// </summary>
        public Angle Course { get; set; }

        /// <summary>
        /// Gets or sets the horizontal accuracy(in meters) of the location.
        /// </summary>
        public double Accuracy { get; set; }

        /// <summary>
        /// Gets or sets the vertical accuracy (in meters) of the location.
        /// </summary>
        public double VerticalAccuracy { get; set; }

        /// <summary>
        /// Gets or sets the date and time of the GNSS position.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Converts latitude and longitude coordinates from decimal degrees format to a GeoPosition object.
        /// </summary>
        /// <param name="latitude">The latitude coordinate in decimal degrees.</param>
        /// <param name="longitude">The longitude coordinate in decimal degrees.</param>
        /// <returns>A GeoPosition object with the specified latitude and longitude coordinates.</returns>
        public static Location FromDecimalDegrees(double latitude, double longitude)
        {
            return new Location()
            {
                Latitude = latitude,
                Longitude = longitude
            };
        }
    }
}