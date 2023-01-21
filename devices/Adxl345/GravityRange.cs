// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Adxl345
{
    /// <summary>
    /// Gravity Measurement Range.
    /// </summary>
    public enum GravityRange
    {
        /// <summary>
        /// Plus/minus 2G.
        /// </summary>
        Range02 = 0x00,

        /// <summary>
        /// Plus/minus 4G.
        /// </summary>
        Range04 = 0x01,

        /// <summary>
        /// Plus/minus 8G.
        /// </summary>
        Range08 = 0x02,

        /// <summary>
        /// Plus/minus 16G.
        /// </summary>
        Range16 = 0x03
    }
}
