// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;

namespace Iot.Device.Common.GnssDevice
{
    /// <summary>
    /// Implements the NMEA0183 data for GPGGA.
    /// </summary>
    public class GpggaData : INmeaData
    {        
        /// <summary>
        /// Gets the location information in Global Navigation Satellite System (GNSS) coordinates.
        /// </summary>
        public GeoPosition Location { get; internal set; }

        /// <inheritdoc/>
        public INmeaData Parse(string inputData)
        {
            try
            {
                var data = inputData.Split(',');
                var lat = data[2];
                var latDir = data[3];
                var lon = data[4];
                var lonDir = data[5];
                var latitude = Nmea0183Parser.ConvertToGeoLocation(lat, latDir);
                var longitude = Nmea0183Parser.ConvertToGeoLocation(lon, lonDir);
                var altitude = double.Parse(data[9]);
                var hdop = double.Parse(data[8]);
                var time = Nmea0183Parser.ConvertToTimeSpan(data[1]);

                var position = GeoPosition.FromDecimalDegrees(latitude, longitude);
                position.Altitude = altitude;
                position.Accuracy = hdop;
                position.Timestamp = DateTime.UtcNow.Date.Add(time);
                return new GpggaData(position);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GpggaData" /> class.
        /// </summary>
        /// <param name="location">A <see cref="GeoPosition"/> item.</param>
        public GpggaData(GeoPosition location)
        {
            Location = location;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GpggaData" /> class.
        /// </summary>
        public GpggaData()
        {
        }
    }
}
