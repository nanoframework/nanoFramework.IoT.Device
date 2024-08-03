// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Common.GnssDevice
{
    /// <summary>
    /// Represents the GNGSA data parsed from NMEA0183 sentences.
    /// </summary>
    public class GngsaData
    {
        /// <summary>
        /// Gets the Gnss module mode.
        /// </summary>
        public GnssOperation Mode { get; }

        /// <summary>
        /// Gets the Gnss module fix status.
        /// </summary>
        public Fix Fix { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GngsaData" /> class.
        /// </summary>
        /// <param name="mode">Gnss mode.</param>
        /// <param name="fix">Gnss fix.</param>
        public GngsaData(GnssOperation mode, Fix fix)
        {
            Mode = mode;
            Fix = fix;
        }
    }
}