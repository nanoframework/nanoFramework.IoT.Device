namespace Iot.Device.ePaperGraphics
{
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
        public bool Color1bpp => !(R <= 0 && G <= 0 && B <= 0);

        /// <summary>
        /// Gets the 4-bits per pixel grayscale value of this color.
        /// </summary>
        public byte Color4bppGrayscale => (byte)((byte)(0.2989 * R + 0.587 * G + 0.114 * B) >> 4);

        /// <summary>
        /// Gets the 8-bits per pixel grayscale value of this color.
        /// </summary>
        public byte Color8bppGrayscale => (byte)(0.2989 * R + 0.587 * G + 0.114 * B);

        /// <summary>
        /// Gets the 8-bits per pixel value of this color.
        /// </summary>
        public byte Color8bppRgb332 => (byte)((R & 0xE0u) | (uint)((G & 0x70) >> 3) | (uint)((B & 0xC0) >> 6));

        /// <summary>
        /// Gets the 12-bits per pixel value of this color.
        /// </summary>
        public ushort Color12bppRgb444 => (ushort)((uint)((R & 0xF0) << 4) | (G & 0xF0u) | (uint)((B & 0xF0) >> 4));

        /// <summary>
        /// Gets the 16-bits per pixel value of this color.
        /// </summary>
        public ushort Color16bppRgb565 => (ushort)(((R & 0xF8) << 8) | ((G & 0xFC) << 3) | (B >> 3));

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

        /// <inheritdoc/>>
        public override bool Equals(object obj)
            => obj is Color other && InternalEquals(this, other);

        /// <inheritdoc/>>
        public override int GetHashCode()
        {
            unchecked
            {
                return R.GetHashCode() ^ G.GetHashCode() ^ B.GetHashCode();
            }
        }

        public static bool operator ==(Color color, Color other)
            => InternalEquals(color, other);

        public static bool operator !=(Color color, Color other)
            => !InternalEquals(color, other);

        private static bool InternalEquals(Color color, Color other) 
            => color.R == other.R && color.G == other.G && color.B == other.B;

        public static Color Black = new(r: 0, g: 0, b: 0);
        public static Color White = new(r: 255, g: 255, b: 255);
        public static Color Red = new Color(r: 255, g: 0, b: 0);
        public static Color Green = new Color(r: 0, g: 255, b: 0);
        public static Color Blue = new Color(r: 0, g: 0, b: 255);
    }
}
