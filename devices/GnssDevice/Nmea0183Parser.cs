// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Iot.Device.Common.GnssDevice
{
    /// <summary>
    /// Provides methods for parsing NMEA0183 data from a Gnss device.
    /// </summary>    
    public static class Nmea0183Parser
    {
        /// <summary>
        /// Gets an array of parsable <see cref="NmeaData"/> parsers.
        /// </summary>
        public static ArrayList MneaDatas { get; } = new ()
        {
            new GllData(),
            new GsaData(),
            new GgaData(),
            new RmcData(),
            new VtgData(),
        };

        /// <summary>
        /// Adds a parser to the list of available parsers.
        /// </summary>
        /// <param name="parser">The parser class.</param>
        public static void AddParser(NmeaData parser)
        {
            MneaDatas.Add(parser);
        }

        /// <summary>
        /// Removes a parser from the list of available parsers.
        /// </summary>
        /// <param name="dataId">The data type to parse eg $GPGLL.</param>
        public static void RemoveParser(string dataId)
        {
            MneaDatas.Remove(dataId);
        }

        /// <summary>
        /// Parses a string and return the parsed NmeaData object.
        /// </summary>
        /// <param name="inputData">A valid MNEA string.</param>
        /// <returns>Parsed NmeaData object if any or null.</returns>
        public static NmeaData Parse(string inputData)
        {
            foreach (NmeaData parser in MneaDatas)
            {
                if (parser.IsMatch(inputData))
                {
                    return parser.Parse(inputData);
                }
            }

            Debug.WriteLine($"No parser found for: {inputData}");
            return null;
        }

        /// <summary>
        /// Converts a string to a geographic location.
        /// </summary>
        /// <param name="data">Valid input data.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="degreesLength">Number of degrees digits.</param>
        /// <returns>A double representing an coordinate elements.</returns>
        public static double ConvertToGeoLocation(string data, string direction, int degreesLength)
        {
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

        /// <summary>
        /// Converts a string to a PositioningIndicator.
        /// </summary>
        /// <param name="data">A valid string.</param>
        /// <returns>The proper <see cref="PositioningIndicator"/> mode.</returns>
        /// <exception cref="Exception">Not a valid positioning.</exception>
        public static PositioningIndicator ConvertToPositioningIndicator(string data)
        {
            switch (data)
            {
                case "A":
                    return PositioningIndicator.Autonomous;
                case "D":
                    return PositioningIndicator.Differential;
                case "E":
                    return PositioningIndicator.Estimated;
                case "M":
                    return PositioningIndicator.Manual;
                case "N":
                    return PositioningIndicator.NotValid;
            }

            throw new Exception();
        }

        /// <summary>
        /// Converts a string to a Status.
        /// </summary>
        /// <param name="data">A valid string.</param>
        /// <returns>The proper <see cref="Status"/>.</returns>
        /// <exception cref="Exception">Not a valid status.</exception>
        public static Status ConvertToStatus(string data)
        {
            switch (data)
            {
                case "A":
                    return Status.Valid;
                case "V":
                    return Status.NotValid;
            }

            throw new Exception();
        }

        /// <summary>
        /// Converts a string date and time to a DateTime.
        /// </summary>
        /// <param name="date">The date string as YYMMDD.</param>
        /// <param name="time">The time as HHMMSS.ss.</param>
        /// <returns>A <see cref="DateTime"/> object.</returns>
        public static DateTime ConvertToUtcDateTime(string date, string time)
        {
            var timespan = ConvertToTimeSpan(time);

            var day = int.Parse(date.Substring(0, 2));
            var month = int.Parse(date.Substring(2, 2));
            var year = int.Parse(date.Substring(4, 2)) + 2000;

            return new DateTime(year, month, day).Add(timespan);
        }

        /// <summary>
        /// Converts a string time to a TimeSpan.
        /// </summary>
        /// <param name="time">The time as HHMMSS.ss.</param>
        /// <returns>A <see cref="TimeSpan"/> object.</returns>
        public static TimeSpan ConvertToTimeSpan(string time)
        {
            var hour = int.Parse(time.Substring(0, 2));
            var minute = int.Parse(time.Substring(2, 2));
            var second = int.Parse(time.Substring(4, 2));
            var millec = int.Parse(time.Substring(7));

            return new TimeSpan(0, hour, minute, second, millec);
        }

        /// <summary>
        /// Computes the checksum of an NMEA1083 message.
        /// </summary>
        /// <param name="data">The string to compute.</param>
        /// <returns>A byte array with the checksum.</returns>
        public static byte ComputeChecksum(string data)
        {
            var checksum = 0;
            foreach (char c in data)
            {
                checksum ^= c;
            }

            return (byte)checksum;
        }
    }
}