// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Common.GnssDevice
{
    /// <summary>
    /// NMEA0183 Data Interface
    /// </summary>
    public interface NmeaData
    {
        /// <summary>
        /// The name of the data type, eg $GNGLL
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The parsing function for the data type
        /// </summary>
        /// <param name="inputData">The input string eg "$GNGSA,M,1,65,67,80,81,82,88,66,,,,,,1.2,0.7,1.0*20"</param>
        /// <returns>An NmeaData</returns>
        public NmeaData Parse(string inputData);

        /// <summary>
        /// Gets the location information in Global Navigation Satellite System (GNSS) coordinates.
        /// </summary>
        public GeoPosition Location { get; }
    }
}
