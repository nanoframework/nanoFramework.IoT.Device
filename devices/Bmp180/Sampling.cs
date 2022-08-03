// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Bmp180
{
    /// <summary>
    /// BMP180 sampling.
    /// </summary>
    public enum Sampling : byte
    {
        /// <summary>
        /// Skipped (output set to 0x80000).
        /// </summary>
        UltraLowPower = 0b000,

        /// <summary>
        /// Oversampling x1.
        /// </summary>
        Standard = 0b001,

        /// <summary>
        /// Oversampling x2.
        /// </summary>
        HighResolution = 0b010,

        /// <summary>
        /// Oversampling x4.
        /// </summary>
        UltraHighResolution = 0b011,
    }
}
