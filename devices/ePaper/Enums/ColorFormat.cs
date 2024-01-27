// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.EPaper.Enums
{
    /// <summary>
    /// Defines the supported color formats in the graphics library.
    /// </summary>
    public enum ColorFormat
    {
        /// <summary>
        /// 1-bit-per-pixel color format.
        /// </summary>
        Color1BitPerPixel,

        /// <summary>
        /// 2-bits-per-pixel. This is used by displays that support 3 colors by using 2 separate RAM.
        /// </summary>
        Color2BitPerPixel,

        /// <summary>
        /// 4-bits-per-pixel color format.
        /// </summary>
        Color4bppGrayscale,

        /// <summary>
        /// 8-bits-per-pixel color format.
        /// </summary>
        Color8bppGrayscale,

        /// <summary>
        /// 8-bits-per-pixel color format.
        /// Colors are encoded in 3 bits for red, 3 bits for green and 2 bits for blue.
        /// </summary>
        Color8bppRgb332,

        /// <summary>
        /// 12-bits-per-pixel color format.
        /// Every color component is encoded in 4 bits.
        /// </summary>
        Color12bppRgb444,

        /// <summary>
        /// 16-bits-per-pixel color format.
        /// Colors are encoded in 5 bits for red, 6 bits for green and 5 bits for blue.
        /// </summary>
        Color16bppRgb565
    }
}
