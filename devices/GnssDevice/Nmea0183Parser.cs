// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Common.GnssDevice
{
    /// <summary>
    /// Provides methods for parsing NMEA0183 data from a Gnss device.
    /// </summary>
    public static class Nmea0183Parser
    {
        /// <summary>
        /// A list of NMEA0183 data objects.
        /// </summary>
        public static NmeaData[] MneaDatas { get; set; }

        /// <summary>
        /// PArses a string and return the parsed NmeaData object.
        /// </summary>
        /// <param name="inputData">A valid MNEA string.</param>
        /// <returns>Parsed NmeaData object if any or null.</returns>
        public static NmeaData Parse(string inputData)
        {
            var data = inputData.Split(',');
            foreach (NmeaData mnea in MneaDatas)
            {
                if (mnea.Name == data[0])
                {
                    return mnea.Parse(inputData);
                }
            }

            return null;
        }

        /// <summary>
        /// Parses the GNGLL NMEA0183 data from a Gnss device.
        /// </summary>
        /// <param name="inputData">The valid GNGLL data string starting with $GNGLL.</param>
        /// <returns>The parsed GngllData object.</returns>
        public static GngllData ParaseGngll(string inputData)
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

        /// <summary>
        /// Parses the GNGSA NMEA0183 data from a Gnss device.
        /// </summary>
        /// <param name="inputData">The valid GNGSA data string starting with $GNGSA.</param>
        /// <returns>The parsed GngsaData object.</returns>
        public static GngsaData ParseGngsa(string inputData)
        {
            var data = inputData.Split(',');
            var mode = ConvertToMode(data[1]);
            var fix = ConvertToFix(data[2]);

            return new GngsaData(mode, fix);
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

        private static GnssOperation ConvertToMode(string data)
        {
            switch (data)
            {
                case "M":
                    return GnssOperation.Manual;
                case "A":
                    return GnssOperation.Auto;
            }

            throw new Exception();
        }

        private static Fix ConvertToFix(string data)
        {
            switch (data)
            {
                case "1":
                    return Fix.NoFix;
                case "2":
                    return Fix.Fix2D;
                case "3":
                    return Fix.Fix3D;
            }

            throw new Exception();
        }
    }
}