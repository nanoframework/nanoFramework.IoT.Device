// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Hdc1080
{
    /// <summary>
    /// Humidity sampling resolution, check data sheet, page 11, 8.5.1.3 section
    /// </summary>
    public enum HumidityResolution : byte
    {
        /// <summary>
        /// 14 bit
        /// </summary>
        High = 0x00,

        /// <summary>
        /// 11 bit
        /// </summary>
        Standard = 0x01,

        /// <summary>
        /// 8 bit
        /// </summary>
        Low = 0x02,
    }
}