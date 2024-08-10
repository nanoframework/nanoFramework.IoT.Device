// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using UnitsNet;

namespace Iot.Device.Common.GnssDevice
{
    /// <summary>
    /// Represents the VTG (Course Over Ground and Ground Speed) NMEA0183 data from a Gnss device.
    /// </summary>
    public class VtgData : NmeaData
    {
        /// <inheritdoc/>
        public override string MessageId => "VTG";

        /// <summary>
        /// Gets the location information in Global Navigation Satellite System (GNSS) coordinates.
        /// </summary>
        public Location Location { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VtgData" /> class.
        /// </summary>
        public VtgData()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VtgData" /> class.
        /// </summary>
        /// <param name="location">A <see cref="Common.GnssDevice.Location"/> element.</param>
        public VtgData(Location location)
        {
            Location = location;
        }

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
                var course = Nmea0183Parser.ConvertToDouble(subfields[1]);
                var speed = Nmea0183Parser.ConvertToDouble(subfields[5]);

                Location position = new Location()
                {
                    Course = Angle.FromDegrees(course),
                    Speed = Speed.FromKnots(speed),
                };

                return new VtgData(position);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
        }
    }
}
