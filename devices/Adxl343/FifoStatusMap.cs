// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.Adxl343Lib
{
    /// <summary>
    /// FIFO Status Map.
    /// </summary>
    internal enum FifoStatusMap
    {
        /// <summary>
        /// FIFO Trigger.
        /// </summary>
        FifoTrig = 0x80,

        /// <summary>
        /// Entries.
        /// </summary>
        Entries = 0x3F
    }
}
