// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.ADXL343Lib
{
    /// <summary>
    /// Tap Axes Map.
    /// </summary>
    internal enum TapAxesMap : byte
    {
        /// <summary>
        /// Suppress.
        /// </summary>
        Suppress = 0x08,

        /// <summary>
        /// Tap X Enable.
        /// </summary>
        TapXEnable = 0x04,

        /// <summary>
        /// Tap Y Enable.
        /// </summary>
        TapYEnable = 0x02,

        /// <summary>
        /// Tap Z Enable.
        /// </summary>
        TapZEnable = 0x01
    }
}
