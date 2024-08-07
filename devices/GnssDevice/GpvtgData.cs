// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using UnitsNet;

namespace Iot.Device.Common.GnssDevice
{
    /// <summary>
    /// Implements the NMEA0183 data for GPVTG.
    /// </summary>
    public class GpvtgData : INmeaData
    {
        /// <summary>
        /// Gets the location information in Global Navigation Satellite System (GNSS) coordinates.
        /// </summary>
        public GeoPosition Location { get; internal set; }

        /// <inheritdoc/>
        public INmeaData Parse(string inputData)
        {
            try
            {
                var data = inputData.Split(',');
                var course = double.Parse(data[1]);
                var speed = double.Parse(data[5]);

                GeoPosition position = new GeoPosition()
                {
                    Course = Angle.FromDegrees(course),
                    Speed = Speed.FromKnots(speed),
                };

                return new GpvtgData(position);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GpvtgData" /> class.
        /// </summary>
        public GpvtgData()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GpvtgData" /> class.
        /// </summary>
        /// <param name="location">A <see cref="GeoPosition"/> element.</param>
        public GpvtgData(GeoPosition location)
        {
            Location = location;
        }
    }
}
