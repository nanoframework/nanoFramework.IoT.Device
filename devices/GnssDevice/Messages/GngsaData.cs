// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;

namespace Iot.Device.Common.GnssDevice
{
    /// <summary>
    /// Represents the GNGSA data parsed from NMEA0183 sentences.
    /// </summary>
    public class GngsaData : INmeaData
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

        /// <summary>
        /// Initializes a new instance of the <see cref="GngsaData" /> class.
        /// </summary>
        public GngsaData()
        {
        }

        /// <inheritdoc/>
        public INmeaData Parse(string inputData)
        {
            var subfields = inputData.Split(',');

            if (subfields[0] != "$GNGSA")
            {
                throw new ArgumentException("GNGSA data is expected.");
            }

            try
            {                
                var mode = Nmea0183Parser.ConvertToMode(subfields[1]);
                var fix = Nmea0183Parser.ConvertToFix(subfields[2]);

                return new GngsaData(mode, fix);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
        }
    }
}