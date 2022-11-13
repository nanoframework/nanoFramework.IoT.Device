// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.EPaper.Primitives
{
    /// <summary>
    /// A color data structure for RGB colors.
    /// </summary>
    public struct Color
    {
        /// <summary>
        /// Gets the red value of this color.
        /// </summary>
        public byte R { get; }

        /// <summary>
        /// Gets the green value of this color.
        /// </summary>
        public byte G { get; }

        /// <summary>
        /// Gets the blue value of this color.
        /// </summary>
        public byte B { get; }

        /// <summary>
        /// Gets a value representing this color as On/Off.
        /// </summary>
        public byte Color1bpp
        {
            get
            {
                return (byte)((R <= 0 && G <= 0 && B <= 0) ? 0x00 : 0xff);
            }
        }

        /// <summary>
        /// Gets the 4-bits per pixel grayscale value of this color.
        /// </summary>
        public byte Color4bppGrayscale
        {
            get
            {
                return (byte)((byte)((0.2989 * R) + (0.587 * G) + (0.114 * B)) >> 4);
            }
        }

        /// <summary>
        /// Gets the 8-bits per pixel grayscale value of this color.
        /// </summary>
        public byte Color8bppGrayscale
        {
            get
            {
                return (byte)((0.2989 * R) + (0.587 * G) + (0.114 * B));
            }
        }

        /// <summary>
        /// Gets the 8-bits per pixel value of this color.
        /// Colors are encoded in 3 bits for red, 3 bits for green and 2 bits for blue.
        /// </summary>
        public byte Color8bppRgb332
        {
            get
            {
                return (byte)((R & 0xE0u) | (uint)((G & 0x70) >> 3) | (uint)((B & 0xC0) >> 6));
            }
        }

        /// <summary>
        /// Gets the 12-bits per pixel value of this color.
        /// All color components are encoded with 4 bits.
        /// </summary>
        public ushort Color12bppRgb444
        {
            get
            {
                return (ushort)((uint)((R & 0xF0) << 4) | (G & 0xF0u) | (uint)((B & 0xF0) >> 4));
            }
        }

        /// <summary>
        /// Gets the 16-bits per pixel value of this color.
        /// Colors are encoded in 5 bits for red, 6 bits for green and 5 bits for blue.
        /// </summary>
        public ushort Color16bppRgb565
        {
            get
            {
                return (ushort)(((R & 0xF8) << 8) | ((G & 0xFC) << 3) | (B >> 3));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> struct.
        /// </summary>
        /// <param name="r">The red value of the color.</param>
        /// <param name="g">The green value of the color.</param>
        /// <param name="b">The blue value of the color.</param>
        public Color(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        /// <summary>
        /// Checks if the specified colors are equivalent based on their <see cref="R"/>, <see cref="G"/>, and <see cref="B"/> values.
        /// </summary>
        /// <param name="color">The first to <see cref="Color"/> to check.</param>
        /// <param name="other">The other <see cref="Color"/> to check.</param>
        /// <returns>True if both instances are the same color value.</returns>
        public static bool operator ==(Color color, Color other)
            => InternalEquals(color, other);

        /// <summary>
        /// Checks if the specified colors are not equivalent based on their <see cref="R"/>, <see cref="G"/>, and <see cref="B"/> values.
        /// </summary>
        /// <param name="color">The first to <see cref="Color"/> to check.</param>
        /// <param name="other">The other <see cref="Color"/> to check.</param>
        /// <returns>True if one color has a different color value than the other.</returns>
        public static bool operator !=(Color color, Color other)
            => !InternalEquals(color, other);

        /// <inheritdoc/>>
        public override bool Equals(object obj)
        {
            return obj is Color other && InternalEquals(this, other);
        }

        /// <inheritdoc/>>
        public override int GetHashCode()
        {
            unchecked
            {
                return R.GetHashCode() ^ G.GetHashCode() ^ B.GetHashCode();
            }
        }

        private static bool InternalEquals(Color color, Color other)
        {
            return color.R == other.R && color.G == other.G && color.B == other.B;
        }

        /// <summary>
        /// Black Color.
        /// </summary>
        public static Color Black = new Color(r: 0, g: 0, b: 0);

        /// <summary>
        /// White Color.
        /// </summary>
        public static Color White = new Color(r: 255, g: 255, b: 255);

        /// <summary>
        /// Red Color.
        /// </summary>
        public static Color Red = new Color(r: 255, g: 0, b: 0);

        /// <summary>
        /// Green Color.
        /// </summary>
        public static Color Green = new Color(r: 0, g: 255, b: 0);

        /// <summary>
        /// Blue Color.
        /// </summary>
        public static Color Blue = new Color(r: 0, g: 0, b: 255);
    }
}
