// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using UnitsNet;

namespace Iot.Device.Common.GnssDevice
{
    /// <summary>
    /// Represents the GPRMC NMEA0183 data from a Gnss device. 
    /// </summary>
    public class GprmcData : INmeaData
    {
        /// <inheritdoc/>
        public INmeaData Parse(string inputData)
        {
            var subfields = inputData.Split(',');
            if (subfields[0] != "$GPRMC")
            {
                throw new ArgumentException("GPRMC data is expected.");
            }

            try
            {
                var status = Nmea0183Parser.ConvertToStatus(subfields[2]);
                var latitude = Nmea0183Parser.ConvertToGeoLocation(subfields[3], subfields[4], 2);
                var longitude = Nmea0183Parser.ConvertToGeoLocation(subfields[5], subfields[6], 3);
                var speed = double.Parse(subfields[7]);
                var course = double.Parse(subfields[8]);
                var datetime = Nmea0183Parser.ConvertToUtcDateTime(subfields[9], subfields[1]);

                var position = GeoPosition.FromDecimalDegrees(latitude, longitude);
                position.Speed = speed;
                position.Course = Angle.FromDegrees(course);
                position.Timestamp = datetime;

                return new GprmcData(position)
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

        /// <summary>
        /// Gets the location information in Global Navigation Satellite System (GNSS) coordinates.
        /// </summary>
        public GeoPosition Location { get; }

        /// <summary>
        /// Gets the data status.
        /// </summary>
        public Status Status { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GprmcData" /> class.
        /// </summary>
        /// <param name="localisarion">A <see cref="GeoPosition"/> element.</param>
        public GprmcData(GeoPosition localisarion)
        {
            Location = localisarion;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GprmcData" /> class.
        /// </summary>
        public GprmcData()
        {
        }
    }
}
