// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;

namespace Iot.Device.Common.GnssDevice
{
    /// <summary>
    /// NMEA0183 Data abstract class.
    /// </summary>
    public abstract class NmeaData
    {
        /// <summary>
        /// Gets the sub fields without the *checksum.
        /// </summary>
        /// <param name="command">The NMEA message to split.</param>
        /// <returns>All the NMEA subfields data.</returns>
        public static string[] GetSubFields(string command)
        {
            var subfields = command.Split(',');
            subfields[subfields.Length - 1] = subfields[subfields.Length - 1].Split('*')[0];
            return subfields;
        }

        /// <summary>
        /// Parse for the specific data type.
        /// </summary>
        /// <param name="inputData">The input data string.</param>
        /// <returns>An NmeaData.</returns>
        public virtual NmeaData Parse(string inputData) => throw new NotImplementedException();

        /// <summary>
        /// Gets the NMEA message ID.
        /// </summary>
        public virtual string MessageId { get; }

        /// <summary>
        /// Determines if the message ID is a match.
        /// </summary>
        /// <param name="inputData">A valid input.</param>
        /// <returns>True if the message ID is a match.</returns>
        public bool IsMatch(string inputData)
        {
            if (inputData.Length < 6)
            {
                return false;
            }

            try
            {
                // Proper message is "$XXAAA,etc"
                return (inputData[0] == '$') && inputData.Substring(3, MessageId.Length) == MessageId && inputData[3 + MessageId.Length] == ',';
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Excption in processing IsMatch with: {inputData}");
            }

            return false;
        }

        /// <summary>
        /// Validate the checksum.
        /// </summary>
        /// <param name="inputData">A valid raw input.</param>
        /// <returns>True if the checksum is correct.</returns>
        public bool ValidateChecksum(string inputData)
        {
            // Remove the start, remove the * and the checksum itself
            var data = inputData.Substring(1, inputData.Length - 4);
            var checksum = inputData.Substring(inputData.Length - 2);
            var compute = Nmea0183Parser.ComputeChecksum(data);
            return compute.ToString("X2") == checksum;
        }

        /// <summary>
        /// Gets the <see cref="GnssMode"/> out of the message.
        /// </summary>
        /// <param name="inputData">A valid input.</param>
        /// <returns>The <see cref="GnssMode"/>.</returns>
        public GnssMode GetGnssMode(string inputData)
        {
            if (!IsMatch(inputData))
            {
                throw new ArgumentException();
            }

            var mode = inputData.Substring(1, 2);
            switch (mode)
            {
                case "GP":
                    return GnssMode.Gps;
                case "GL":
                    return GnssMode.Glonass;
                case "GA":
                    return GnssMode.Galileo;
                case "BD":
                    return GnssMode.BeiDou;
                case "GI":
                    return GnssMode.NavIC;
                case "GN":
                    return GnssMode.Gnss;
                case "CQ":
                    return GnssMode.Qzss;
                default:
                    return GnssMode.Other;
            }
        }
    }
}
