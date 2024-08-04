// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Common.GnssDevice
{
    /// <summary>
    /// NMEA0183 Data Interface.
    /// </summary>
    public interface INmeaData
    {
        /// <summary>
        /// Parse for the specific data type.
        /// </summary>
        /// <param name="inputData">The input data string.</param>
        /// <returns>An NmeaData.</returns>
        public INmeaData Parse(string inputData);
    }
}
