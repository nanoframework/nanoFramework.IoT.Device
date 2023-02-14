// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.ADXL343Lib
{
    /// <summary>
    /// FIFO Control Map.
    /// </summary>
    internal enum FifoControlMap : byte
    {
        /// <summary>
        /// Mode.
        /// </summary>
        Mode = 0xC0,

        /// <summary>
        /// Trigger.
        /// </summary>
        Trigger = 0x20,

        /// <summary>
        /// Samples.
        /// </summary>
        Samples = 0x0F
    }
}
