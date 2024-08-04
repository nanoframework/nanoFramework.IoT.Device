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
        /// Gets or sets an array of parsable <see cref="MneaDatas"/> parsers.
        /// </summary>
        public static INmeaData[] MneaDatas { get; set; } = new INmeaData[]
        {
            new GpgllData(),
            new GngsaData()
        };

        /// <summary>
        /// PArses a string and return the parsed NmeaData object.
        /// </summary>
        /// <param name="inputData">A valid MNEA string.</param>
        /// <returns>Parsed NmeaData object if any or null.</returns>
        public static INmeaData Parse(string inputData)
        {
            var data = inputData.Split(',');
            foreach (INmeaData mnea in MneaDatas)
            {
                if (mnea.Name == data[0])
                {
                    return mnea.Parse(inputData);
                }
            }

            return null;
        }

        /// <summary>
        /// Converts a string to a geographic location.
        /// </summary>
        /// <param name="data">Valid input data.</param>
        /// <param name="direction">The direction.</param>
        /// <returns>A double representing an coordinate elements.</returns>
        public static double ConvertToGeoLocation(string data, string direction)
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

        /// <summary>
        /// Converts a string to a GnssOperation.
        /// </summary>
        /// <param name="data">A valid string.</param>
        /// <returns>A <see cref="GnssOperation"/>.</returns>
        public static GnssOperation ConvertToMode(string data)
        {
            switch (data)
            {
                case "M":
                    return GnssOperation.Manual;
                case "A":
                    return GnssOperation.Auto;
                default:
                    return GnssOperation.Unknown;
            }
        }

        /// <summary>
        /// Converts a string to a Fix.
        /// </summary>
        /// <param name="data">A valid string.</param>
        /// <returns>A <see cref="Fix"/>.</returns>
        /// <exception cref="Exception">Not a valid fix.</exception>
        public static Fix ConvertToFix(string data)
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