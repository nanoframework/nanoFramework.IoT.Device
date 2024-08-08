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
        public GeoPosition Location { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VtgData" /> class.
        /// </summary>
        public VtgData()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VtgData" /> class.
        /// </summary>
        /// <param name="location">A <see cref="GeoPosition"/> element.</param>
        public VtgData(GeoPosition location)
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

            try
            {
                var subfields = inputData.Split(',');
                var course = double.Parse(subfields[1]);
                var speed = double.Parse(subfields[5]);

                GeoPosition position = new GeoPosition()
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
