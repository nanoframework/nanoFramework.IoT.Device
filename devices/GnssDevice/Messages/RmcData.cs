// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using UnitsNet;

namespace Iot.Device.Common.GnssDevice
{
    /// <summary>
    /// Represents the RMC (Recommended Minimum Specific GNSS Data) NMEA0183 data from a Gnss device.
    /// </summary>
    public class RmcData : NmeaData
    {
        /// <inheritdoc/>
        public override string MessageId => "RMC";

        /// <summary>
        /// Gets the location information in Global Navigation Satellite System (GNSS) coordinates.
        /// </summary>
        public GeoPosition Location { get; }

        /// <summary>
        /// Gets the data status.
        /// </summary>
        public Status Status { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RmcData" /> class.
        /// </summary>
        /// <param name="localisarion">A <see cref="GeoPosition"/> element.</param>
        public RmcData(GeoPosition localisarion)
        {
            Location = localisarion;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RmcData" /> class.
        /// </summary>
        public RmcData()
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
                var status = Nmea0183Parser.ConvertToStatus(subfields[2]);
                var latitude = Nmea0183Parser.ConvertToGeoLocation(subfields[3], subfields[4], 2);
                var longitude = Nmea0183Parser.ConvertToGeoLocation(subfields[5], subfields[6], 3);
                var speed = Nmea0183Parser.ConvertToDouble(subfields[7]);
                var course = Nmea0183Parser.ConvertToDouble(subfields[8]);

                var datetime = Nmea0183Parser.ConvertToUtcDateTime(subfields[9], subfields[1]);

                var position = GeoPosition.FromDecimalDegrees(latitude, longitude);
                position.Speed = Speed.FromKnots(speed);
                position.Course = Angle.FromDegrees(course);
                position.Timestamp = datetime;

                return new RmcData(position)
                {
                    Status = status,
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
