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

            try
            {
                var subfields = inputData.Split(',');
                subfields[subfields.Length - 1] = subfields[subfields.Length - 1].Split('*')[0];

                var sats = new int[12];
                for (int i = 0; i < 12; i++)
                {
                    if (subfields[i + 3] != string.Empty)
                    {
                        sats[i] = int.Parse(subfields[i + 3]);
                    }
                }

                var gsa = new GsaData()
                {
                    OperationMode = Nmea0183Parser.ConvertToMode(subfields[1]),
                    Fix = Nmea0183Parser.ConvertToFix(subfields[2]),
                    SatellitesInUse = sats,
                    PositionDilutionOfPrecision = double.Parse(subfields[15]),
                    HorizontalDilutionOfPrecision = double.Parse(subfields[16]),
                    VerticalDilutionOfPrecision = double.Parse(subfields[17].Split('*')[0]),
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
