// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Common.GnssDevice
{
    /// <summary>
    /// Represents the ZDA (Timing MEssage) NMEA0183 data from a Gnss device.
    /// </summary>
    public class ZdaData : NmeaData
    {
        /// <inheritdoc/>
        public override string MessageId => "ZDA";

        /// <summary>
        /// Gets the time of the GNSS position.
        /// </summary>
        public DateTime DateTime { get; internal set; }

        /// <summary>
        /// Gets the local hour of the GNSS position.
        /// </summary>
        public int LocalHour { get; internal set; }

        /// <summary>
        /// Gets the local minute of the GNSS position.
        /// </summary>
        public int LocalMinute { get; internal set; }

        /// <inheritdoc/>
        public override NmeaData Parse(string inputData)
        {
            if (!IsMatch(inputData))
            {
                throw new ArgumentException();
            }

            if (!ValidateChecksum(inputData))
            {
                return null;
            }

            try
            {
                var subfields = GetSubFields(inputData);
                var time = Nmea0183Parser.ConvertToTimeSpan(subfields[1]);
                var day = Nmea0183Parser.ConvertToInt(subfields[2]);
                var month = Nmea0183Parser.ConvertToInt(subfields[3]);
                var year = Nmea0183Parser.ConvertToInt(subfields[4]);
                var localHour = Nmea0183Parser.ConvertToInt(subfields[5]);
                var localMinute = Nmea0183Parser.ConvertToInt(subfields[6]);

                return new ZdaData()
                {
                    DateTime = new DateTime(year, month, day).Add(time),
                    LocalHour = localHour,
                    LocalMinute = localMinute,
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
