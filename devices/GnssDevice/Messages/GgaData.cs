// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;

namespace Iot.Device.Common.GnssDevice
{
    /// <summary>
    /// Represents the GGA (Global Positioning System Fix Data) NMEA0183 data from a Gnss device.
    /// </summary>
    public class GgaData : NmeaData
    {
        /// <inheritdoc/>
        public override string MessageId => "GGA";

        /// <summary>
        /// Gets the location information in Global Navigation Satellite System (GNSS) coordinates.
        /// </summary>
        public GeoPosition Location { get; internal set; }

        /// <summary>
        /// Gets the number of satellites in use.
        /// </summary>
        public int SatellitesInView { get; internal set; }

        /// <summary>
        /// Gets the geodetic separation.
        /// </summary>
        public double GeodidSeparation { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GgaData" /> class.
        /// </summary>
        /// <param name="location">A <see cref="GeoPosition"/> item.</param>
        public GgaData(GeoPosition location)
        {
            Location = location;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GgaData" /> class.
        /// </summary>
        public GgaData()
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
                var subfields = GetSubFields(inputData);
                var lat = subfields[2];
                var latDir = subfields[3];
                var lon = subfields[4];
                var lonDir = subfields[5];
                var latitude = Nmea0183Parser.ConvertToGeoLocation(lat, latDir, 2);
                var longitude = Nmea0183Parser.ConvertToGeoLocation(lon, lonDir, 3);
                var altitude = double.Parse(subfields[9]);
                var hdop = double.Parse(subfields[8]);
                var time = Nmea0183Parser.ConvertToTimeSpan(subfields[1]);

                var position = GeoPosition.FromDecimalDegrees(latitude, longitude);
                position.Altitude = altitude;
                position.Accuracy = hdop;
                position.Timestamp = DateTime.UtcNow.Date.Add(time);
                return new GgaData(position)
                {
                    SatellitesInView = int.Parse(subfields[7]),
                    GeodidSeparation = double.Parse(subfields[11]),
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
