// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.ADXL343Lib
{
    /// <summary>
    /// Data Format Map.
    /// </summary>
    internal enum DataFormatMap
    {
        /// <summary>
        /// Self Test.
        /// </summary>
        SelfTest = 0x80,

        /// <summary>
        /// SPI.
        /// </summary>
        Spi = 0x40,

        /// <summary>
        /// Interrupt Invert.
        /// </summary>
        IntInvert = 0x20,

        /// <summary>
        /// Reserved.
        /// </summary>
        Reserved = 0x10,

        /// <summary>
        /// Full Resolution.
        /// </summary>
        FullRes = 0x08,

        /// <summary>
        /// Justify.
        /// </summary>
        Justify = 0x04,

        /// <summary>
        /// Range.
        /// </summary>
        Range = 0x03
    }
}
