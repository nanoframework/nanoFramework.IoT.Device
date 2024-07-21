// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Common.GpsDevice
{
    /// <summary>
    /// Represents a geographic position with latitude and longitude coordinates.
    /// </summary>
    public class GeoPosition
    {
        /// <summary>
        /// Gets the latitude of a geographic position.
        /// </summary>
        public double Latitude { get; private set; }
        
        /// <summary>
        /// Gets the longitude of a geographic position.
        /// </summary>
        public double Longitude { get; private set; }

        /// <summary>
        /// Converts latitude and longitude coordinates from decimal degrees format to a GeoPosition object.
        /// </summary>
        /// <param name="latitude">The latitude coordinate in decimal degrees.</param>
        /// <param name="longitude">The longitude coordinate in decimal degrees.</param>
        /// <returns>A GeoPosition object with the specified latitude and longitude coordinates.</returns>
        public static GeoPosition FromDecimalDegrees(double latitude, double longitude)
        {
            return new GeoPosition()
            {
                Latitude = latitude,
                Longitude = longitude
            };
        }
    }
}