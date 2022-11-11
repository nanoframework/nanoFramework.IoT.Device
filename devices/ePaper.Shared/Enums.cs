// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.ePaper.Shared
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
        Color2BitPerPixel
    }
}
