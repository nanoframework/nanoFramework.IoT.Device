// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Iot.Device.EPaper.Primitives
{
    /// <summary>
    /// Augments the <see cref="Color"/> class with additional helper methods.
    /// </summary>
    public static class ColorExtensions
    {
        /// <summary>
        /// Gets a value representing this color as On/Off.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> to work with.</param>
        /// <returns>The 1-bit-per-pixel value of the specified <see cref="Color"/></returns>
        public static byte GetAs1bpp(this Color color)
        {
            return (byte)((color.R <= 0 && color.G <= 0 && color.B <= 0) ? 0x00 : 0xff);
        }

        /// <summary>
        /// Gets the 4-bits per pixel grayscale value of this color.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> to work with.</param>
        /// <returns>The 4-bit-per-pixel grayscale value of the specified <see cref="Color"/></returns>
        public static byte GetAs4bppGrayscale(this Color color)
        {
            return (byte)((byte)((0.2989 * color.R) + (0.587 * color.G) + (0.114 * color.B)) >> 4);
        }

        /// <summary>
        /// Gets the 8-bits per pixel grayscale value of this color.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> to work with.</param>
        /// <returns>The 8-bit-per-pixel grayscale value of the specified <see cref="Color"/></returns>
        public static byte GetAs8bppGrayscale(this Color color)
        {
            return (byte)((0.2989 * color.R) + (0.587 * color.G) + (0.114 * color.B));
        }

        /// <summary>
        /// Gets the 8-bits per pixel value of this color.
        /// Colors are encoded in 3 bits for red, 3 bits for green and 2 bits for blue.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> to work with.</param>
        /// <returns>The 8-bit-per-pixel RGB332 value of the specified <see cref="Color"/></returns>
        public static byte GetAs8bppRgb332(this Color color)
        {
            return (byte)((color.R & 0xE0u) | (uint)((color.G & 0x70) >> 3) | (uint)((color.B & 0xC0) >> 6));
        }

        /// <summary>
        /// Gets the 12-bits per pixel value of this color.
        /// All color components are encoded with 4 bits.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> to work with.</param>
        /// <returns>The 12-bit-per-pixel RGB444 value of the specified <see cref="Color"/></returns>
        public static ushort GetAs12bppRgb444(this Color color)
        {
            return (ushort)((uint)((color.R & 0xF0) << 4) | (color.G & 0xF0u) | (uint)((color.B & 0xF0) >> 4));
        }

        /// <summary>
        /// Gets the 16-bits per pixel value of this color.
        /// Colors are encoded in 5 bits for red, 6 bits for green and 5 bits for blue.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> to work with.</param>
        /// <returns>The 16-bit-per-pixel RGB565 value of the specified <see cref="Color"/></returns>
        public static ushort Color16bppRgb565(this Color color)
        {
            return (ushort)(((color.R & 0xF8) << 8) | ((color.G & 0xFC) << 3) | (color.B >> 3));
        }
    }
}
