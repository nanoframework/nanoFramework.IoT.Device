// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;

namespace Iot.Device.Common.GnssDevice
{
    /// <summary>
    /// Represents the GSA (GNSS DOP and Active Satellites) NMEA0183 data from a Gnss device.
    /// </summary>
    public class GsaData : NmeaData
    {
        /// <inheritdoc/>
        public override string MessageId => "GSA";

        /// <summary>
        /// Initializes a new instance of the <see cref="GsaData"/> class.
        /// </summary>
        public GsaData()
        {
        }

        /// <summary>
        /// Gets the operation mode of the Gnss module.
        /// </summary>
        public GnssOperation OperationMode { get; private set; }

        /// <summary>
        /// Gets the Gnss module fix status.
        /// </summary>
        public Fix Fix { get; private set; }

        /// <summary>
        /// Gets the positioning indicator.
        /// </summary>
        public PositioningIndicator PositioningIndicator { get; private set; }

        /// <summary>
        /// Gets the satellite in use.
        /// </summary>
        public int[] SatellitesInUse { get; private set; }

        /// <summary>
        /// Gets or sets the Position Dilution of Precision of the GNSS position.
        /// </summary>
        public double PositionDilutionOfPrecision { get; set; }

        /// <summary>
        /// Gets or sets the Horizontal Dilution of Precision of the GNSS position.
        /// </summary>
        public double HorizontalDilutionOfPrecision { get; set; }

        /// <summary>
        /// Gets or sets the Vertical Dilution of Precision of the GNSS position.
        /// </summary>
        public double VerticalDilutionOfPrecision { get; set; }

        /// <summary>
        /// Parse the GPGSA data.
        /// </summary>
        /// <param name="inputData">The input data string.</param>
        /// <returns>An NmeaData.</returns>
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

                var sats = new int[12];
                for (int i = 0; i < 12; i++)
                {
                    sats[i] = Nmea0183Parser.ConvertToInt(subfields[i + 3]);
                }

                var gsa = new GsaData()
                {
                    OperationMode = Nmea0183Parser.ConvertToMode(subfields[1]),
                    Fix = Nmea0183Parser.ConvertToFix(subfields[2]),
                    SatellitesInUse = sats,
                    PositionDilutionOfPrecision = Nmea0183Parser.ConvertToDouble(subfields[15]),
                    HorizontalDilutionOfPrecision = Nmea0183Parser.ConvertToDouble(subfields[16]),
                    VerticalDilutionOfPrecision = Nmea0183Parser.ConvertToDouble(subfields[17]),
                };

                return gsa;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
        }
    }
}
