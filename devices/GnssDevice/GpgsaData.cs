// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;

namespace Iot.Device.Common.GnssDevice
{
    /// <summary>
    /// Represents the GPGSA NMEA0183 data from a Gnss device.
    /// </summary>
    public class GpgsaData : INmeaData
    {
        /// <summary>
        /// Initializes a new instance of the GpgsaData class.
        /// </summary>
        public GpgsaData()
        {
        }

        /// <summary>
        /// Gets the operation mode of the Gnss module.
        /// </summary>
        public GnssOperation OperationMode { get; private set; }

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
        public INmeaData Parse(string inputData)
        {
            string[] subfields = inputData.Split(',');
            if (subfields[0] != "$GPGSA")
            {
                throw new ArgumentException("GPGSA data is expected.");
            }

            try
            {
                var sats = new int[12];
                for (int i = 0; i < 12; i++)
                {
                    sats[i] = int.Parse(subfields[i + 3]);
                }

                var gsa = new GpgsaData()
                {
                    OperationMode = Nmea0183Parser.ConvertToMode(subfields[1]),
                    PositioningIndicator = Nmea0183Parser.ConvertToPositioningIndicator(subfields[2]),
                    SatellitesInUse = sats,
                    PositionDilutionOfPrecision = double.Parse(subfields[15]),
                    HorizontalDilutionOfPrecision = double.Parse(subfields[16]),
                    VerticalDilutionOfPrecision = double.Parse(subfields[17]),
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
