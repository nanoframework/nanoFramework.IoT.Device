// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.Adxl343Lib
{
    /// <summary>
    /// FIFO Mode.
    /// </summary>
    public enum FifoMode : byte
    {
        /// <summary>
        /// Bypass.
        /// </summary>
        Bypass = 0x00,

        /// <summary>
        /// FIFO.
        /// </summary>
        Fifo = 0x01,

        /// <summary>
        /// Stream.
        /// </summary>
        Stream = 0x02,

        /// <summary>
        /// Trigger.
        /// </summary>
        Trigger = 0x03,
    }
}
