// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ft4222
{
    /// <summary>
    /// Intensity for SPI in Milli Amperes
    /// </summary>
    internal enum SpiDrivingStrength
    {
        /// <summary>
        /// 4 Milli Amperes
        /// </summary>
        Intensity4mA = 0,

        /// <summary>
        /// 8 Milli Amperes
        /// </summary>
        Intensity8mA,

        /// <summary>
        /// 12 Milli Amperes
        /// </summary>
        Intensity12mA,

        /// <summary>
        /// 16 Milli Amperes
        /// </summary>
        Intensity16mA,
    }
}
