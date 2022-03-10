// This is a class added speifically for nanoFramework so we dont need to include the full fat ImageSharp lib!

using System;

namespace SixLabors.ImageSharp
{
    /// <summary>
    /// Stores an ordered pair of integers, which specify a height and width.
    /// </summary>
    /// <remarks>
    /// This struct is fully mutable. This is done (against the guidelines) for the sake of performance,
    /// as it avoids the need to create new values for modification operations.
    /// </remarks>
    public struct Size
    {
        /// <summary>
        /// Gets or sets the height of this <see cref="Size"/>.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the width of this <see cref="Size"/>.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Size"/> struct.
        /// </summary>
        /// <param name="width">The width of the size.</param>
        /// <param name="height">The height of the size.</param>
        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }

    ///// <content>
    ///// Contains static named color values.
    ///// <see href="https://www.w3.org/TR/css-color-3/"/>
    ///// </content>
    //public class Color
    //{
    //    /// <summary>
    //    /// Represents a <see paramref="Color"/> matching the W3C definition that has an hex value of #000000.
    //    /// </summary>
    //    public static readonly Color Black = FromRgba(0, 0, 0, 255);


    //    /// <summary>
    //    /// Represents a <see paramref="Color"/> matching the W3C definition that has an hex value of #FFFFFF.
    //    /// </summary>
    //    public static readonly Color White = FromRgba(255, 255, 255, 255);

    //    /// <summary>
    //    /// Creates a <see cref="Color"/> from RGBA bytes.
    //    /// </summary>
    //    /// <param name="r">The red component (0-255).</param>
    //    /// <param name="g">The green component (0-255).</param>
    //    /// <param name="b">The blue component (0-255).</param>
    //    /// <param name="a">The alpha component (0-255).</param>
    //    /// <returns>The <see cref="Color"/>.</returns>
    //    public static Color FromRgba(byte r, byte g, byte b, byte a) => new(r, g, b, a);

    //    public static int ToPixel(int color)
    //    {
    //        return color;
    //    }

    //}
}
