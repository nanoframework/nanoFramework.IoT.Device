// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Common.GnssDevice
{
    /// <summary>
    /// Represents the GNGLL NMEA0183 data from a Gnss device.
    /// </summary>
    public class GngllData : INmeaData
    {
        /// <inheritdoc/>
        public string Name => "$GPGLL";

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

                return new GngllData(GeoPosition.FromDecimalDegrees(latitude, longitude));
            }
            catch
            {
            }

            return null;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="GngllData" /> class.
        /// </summary>
        public GngllData()
        {
        }
    }
}