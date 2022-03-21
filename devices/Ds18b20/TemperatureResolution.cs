// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ds18b20
{
    /// <summary>
    /// Temperature sampling resolution, check data sheet, page 11, 8.5.1.3 section
    /// </summary>
    public enum TemperatureResolution : byte
    {
        /// <summary>
        /// 9 bit
        /// </summary>
        VeryLow = 0x00,

        /// <summary>
        /// 10 bit
        /// </summary>
        Low = 0x01,

        /// <summary>
        ///11 bit
        /// </summary>
        High = 0x02,

        /// <summary>
        /// 12 bit
        /// </summary>
        VeryHigh = 0x03,
    }
}