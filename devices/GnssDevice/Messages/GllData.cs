// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;

namespace Iot.Device.Common.GnssDevice
{
    /// <summary>
    /// Represents the GLL (Geographic Position - Latitude/Longitude) NMEA0183 data from a Gnss device.
    /// </summary>
    public class GllData : NmeaData
    {
        /// <inheritdoc/>
        public override string MessageId => "GLL";

        /// <summary>
        /// Gets the location information in Global Navigation Satellite System (GNSS) coordinates.
        /// </summary>
        public GeoPosition Location { get; }

        /// <summary>
        /// Gets the data status.
        /// </summary>
        public Status Status { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GllData" /> class.
        /// </summary>
        /// <param name="location">Location information.</param>
        public GllData(GeoPosition location)
        {
            Location = location;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GllData" /> class.
        /// </summary>
        public GllData()
        {
        }

        /// <inheritdoc/>
        public override NmeaData Parse(string inputData)
        {
            if (!IsMatch(inputData))
            {
                throw new ArgumentException();
            }

            try
            {
                var subfields = inputData.Split(',');
                var lat = subfields[1];
                var latDir = subfields[2];
                var lon = subfields[3];
                var lonDir = subfields[4];
                var latitude = Nmea0183Parser.ConvertToGeoLocation(lat, latDir, 2);
                var longitude = Nmea0183Parser.ConvertToGeoLocation(lon, lonDir, 3);
                var time = Nmea0183Parser.ConvertToTimeSpan(subfields[5]);

                var geo = GeoPosition.FromDecimalDegrees(latitude, longitude);
                geo.Timestamp = DateTime.UtcNow.Date.Add(time);
                return new GllData(geo)
                {
                    Status = Nmea0183Parser.ConvertToStatus(subfields[6]),
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
        }    
    }
}