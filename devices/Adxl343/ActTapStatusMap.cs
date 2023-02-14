// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.ADXL343Lib
{
    /// <summary>
    /// Active Tap Status Map.
    /// </summary>
    internal enum ActTapStatusMap
    {
        /// <summary>
        /// Active X Source.
        /// </summary>
        ActXSource = 0x40,

        /// <summary>
        /// Active Y Source.
        /// </summary>
        ActYSource = 0x20,

        /// <summary>
        /// Active Z Source.
        /// </summary>
        ActZSource = 0x10,

        /// <summary>
        /// Asleep.
        /// </summary>
        Asleep = 0x08,

        /// <summary>
        /// Tap X Source.
        /// </summary>
        TapXSource = 0x04,

        /// <summary>
        /// Tap Y Source.
        /// </summary>
        TapYSource = 0x02,

        /// <summary>
        /// Tap Z Source.
        /// </summary>
        TapZSource = 0x01
    }
}
