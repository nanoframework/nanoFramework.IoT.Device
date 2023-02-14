// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.ADXL343Lib
{
    /// <summary>
    /// Inactive Enable Map.
    /// </summary>
    internal enum IntEnableMap
    {
        /// <summary>
        /// Data Ready.
        /// </summary>
        DataReady = 0x80,

        /// <summary>
        /// Single Tap.
        /// </summary>
        SingleTap = 0x40,

        /// <summary>
        /// Double Tap.
        /// </summary>
        DoubleTap = 0x20,

        /// <summary>
        /// Activity.
        /// </summary>
        Activity = 0x10,

        /// <summary>
        /// Inactivity.
        /// </summary>
        Inactivity = 0x08,

        /// <summary>
        /// Freefall.
        /// </summary>
        Freefall = 0x04,

        /// <summary>
        /// Watermark.
        /// </summary>
        Watermark = 0x02,

        /// <summary>
        /// Overrun.
        /// </summary>
        Overrun = 0x01
    }
}
