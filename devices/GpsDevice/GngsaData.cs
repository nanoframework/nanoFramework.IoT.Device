// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Common.GpsDevice
{
    /// <summary>
    /// Represents the GNGSA data parsed from NMEA0183 sentences.
    /// </summary>
    public class GngsaData
    {
        /// <summary>
        /// Gets the GPS module mode.
        /// </summary>
        public Mode Mode { get; }

        /// <summary>
        /// Gets the GPS module fix status.
        /// </summary>
        public Fix Fix { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GngsaData" /> class.
        /// </summary>
        /// <param name="mode">GPS mode.</param>
        /// <param name="fix">GPS fix.</param>
        public GngsaData(Mode mode, Fix fix)
        {
            Mode = mode;
            Fix = fix;
        }
    }
}