// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Magnetometer
{
    /// <summary>
    /// Represents the trim registers of the sensor (trim values in the "trim_data" of device structure).
    /// </summary>
    public class Bmm150TrimRegisterData
    {
        /// <summary>
        /// Gets or sets trim DigX1 data.
        /// </summary>
        public byte DigX1 { get; set; }

        /// <summary>
        /// Gets or sets trim DigY1 data.
        /// </summary>
        public byte DigY1 { get; set; }

        /// <summary>
        /// Gets or sets trim DigX2 data.
        /// </summary>
        public byte DigX2 { get; set; }

        /// <summary>
        /// Gets or sets trim DigY2 data.
        /// </summary>
        public byte DigY2 { get; set; }

        /// <summary>
        /// Gets or sets trim DigZ1 data.
        /// </summary>
        public int DigZ1 { get; set; }

        /// <summary>
        /// Gets or sets trim DigZ2 data.
        /// </summary>
        public int DigZ2 { get; set; }

        /// <summary>
        /// Gets or sets trim DigZ3 data.
        /// </summary>
        public int DigZ3 { get; set; }

        /// <summary>
        /// Gets or sets trim DigZ4 data.
        /// </summary>
        public int DigZ4 { get; set; }

        /// <summary>
        /// Gets or sets trim DigXy1 data.
        /// </summary>
        public int DigXy1 { get; set; }

        /// <summary>
        /// Gets or sets trim DigXy2 data.
        /// </summary>
        public int DigXy2 { get; set; }

        /// <summary>
        /// Gets or sets trim DigXyz1 data.
        /// </summary>
        public int DigXyz1 { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bmm150TrimRegisterData" /> class.
        /// </summary>
        /// <param name="trimX1y1Data">TrimX1y1Data bytes.</param>
        /// <param name="trimXyzData">TrimXyzData bytes.</param>
        /// <param name="trimXy1Xy2Data">TrimXy1Xy2Data bytes.</param>
        public Bmm150TrimRegisterData(Span<byte> trimX1y1Data, Span<byte> trimXyzData, Span<byte> trimXy1Xy2Data)
        {
            DigX1 = (byte)trimX1y1Data[0];
            DigY1 = (byte)trimX1y1Data[1];
            DigX2 = (byte)trimXyzData[2];
            DigY2 = (byte)trimXyzData[3];
            DigZ1 = trimXy1Xy2Data[3] << 8 | trimXy1Xy2Data[2];
            DigZ2 = (short)(trimXy1Xy2Data[1] << 8 | trimXy1Xy2Data[0]);
            DigZ3 = (short)(trimXy1Xy2Data[7] << 8 | trimXy1Xy2Data[6]);
            DigZ4 = (short)(trimXyzData[1] << 8 | trimXyzData[0]);
            DigXy1 = trimXy1Xy2Data[9];
            DigXy2 = (sbyte)trimXy1Xy2Data[8];
            DigXyz1 = ((trimXy1Xy2Data[5] & 0x7F) << 8) | trimXy1Xy2Data[4];
        }
    }
}