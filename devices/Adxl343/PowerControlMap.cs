// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.Adxl343Lib
{
    /// <summary>
    /// Power control Map.
    /// </summary>
    internal enum PowerControlMap
    {
        /// <summary>
        /// Link.
        /// </summary>
        Link = 0x20,

        /// <summary>
        /// Auto Sleep.
        /// </summary>
        AutoSleep = 0x10,

        /// <summary>
        /// Measure.
        /// </summary>
        Measure = 0x08,

        /// <summary>
        /// Sleep.
        /// </summary>
        Sleep = 0x04,

        /// <summary>
        /// Wakeup.
        /// </summary>
        Wakeup = 0x03
    }
}
