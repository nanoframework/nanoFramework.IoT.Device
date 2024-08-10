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
        public Location Location { get; internal set; }

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
        /// <param name="location">A <see cref="Common.GnssDevice.Location"/> item.</param>
        public GgaData(Location location)
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
                var altitude = Nmea0183Parser.ConvertToDouble(subfields[9]);
                var hdop = Nmea0183Parser.ConvertToDouble(subfields[8]);
                var time = Nmea0183Parser.ConvertToTimeSpan(subfields[1]);

                var position = Location.FromDecimalDegrees(latitude, longitude);
                position.Altitude = altitude;
                position.Accuracy = hdop;
                position.Timestamp = DateTime.UtcNow.Date.Add(time);
                return new GgaData(position)
                {
                    SatellitesInView = Nmea0183Parser.ConvertToInt(subfields[7]),
                    GeodidSeparation = Nmea0183Parser.ConvertToDouble(subfields[11]),
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
