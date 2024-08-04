// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;

namespace Iot.Device.Common.GnssDevice
{
    /// <summary>
    /// Represents the GNGLL NMEA0183 data from a Gnss device.
    /// </summary>
    public class GpgllData : INmeaData
    {
        /// <inheritdoc/>
        public INmeaData Parse(string inputData)
        {
            try
            {
                var data = inputData.Split(',');
                var lat = data[1];
                var latDir = data[2];
                var lon = data[3];
                var lonDir = data[4];
                var latitude = Nmea0183Parser.ConvertToGeoLocation(lat, latDir);
                var longitude = Nmea0183Parser.ConvertToGeoLocation(lon, lonDir);

                return new GpgllData(GeoPosition.FromDecimalDegrees(latitude, longitude));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Gets the location information in Global Navigation Satellite System (GNSS) coordinates.
        /// </summary>
        public GeoPosition Location { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GpgllData" /> class.
        /// </summary>
        /// <param name="location">Location information.</param>
        public GpgllData(GeoPosition location)
        {
            Location = location;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GpgllData" /> class.
        /// </summary>
        public GpgllData()
        {
        }
    }
}