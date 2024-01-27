// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.Adxl343Lib
{
    /// <summary>
    /// Gravity Range.
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
