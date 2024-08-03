// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Common.GnssDevice
{
    /// <summary>
    /// Represents the GNGLL NMEA0183 data from a Gnss device.
    /// </summary>
    public class GngllData: NmeaData
    {
        /// <summary>
        /// The name of the data type, eg $GNGLL
        /// </summary>
        public string Name => "$GNGLL";

        /// <summary>
        /// Parses the GNGLL NMEA0183 data from a Gnss device.
        /// </summary>
        /// <param name="inputData">The valid GNGLL data string starting with $GNGLL.</param>
        /// <returns>The parsed GngllData object.</returns>
        public NmeaData Parse(string inputData)
        {
            var data = inputData.Split(',');
            var lat = data[1];
            var latDir = data[2];
            var lon = data[3];
            var lonDir = data[4];
            var latitude = ConvertToGeoLocation(lat, latDir);
            var longitude = ConvertToGeoLocation(lon, lonDir);

            return new GngllData(GeoPosition.FromDecimalDegrees(latitude, longitude));
        }

        private static double ConvertToGeoLocation(string data, string direction)
        {
            var degreesLength = data.Length > 10 ? 3 : 2;

            var degrees = double.Parse(data.Substring(0, degreesLength));
            var minutes = double.Parse(data.Substring(degreesLength));

            var result = degrees + (minutes / 60);

            if (direction == "S" || direction == "W")
            {
                return -result;
            }

            return result;
        }

        /// <inheritdoc/>
        public GeoPosition Location { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GngllData" /> class.
        /// </summary>
        /// <param name="location">Location information.</param>
        public GngllData(GeoPosition location)
        {
            Location = location;
        }
    }
}