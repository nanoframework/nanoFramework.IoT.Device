//
// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
//

namespace Iot.Device.CharacterLcd
{
    /// <summary>
    /// Represents a rectangle with all the required attributes.
    /// </summary>
    public struct Rectangle
    {
        /// <summary>
        /// Gets or sets the X-coordinate.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Gets or sets the Y-coordinate.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// An empty representation of a <see cref="Rectangle"/> with <see cref="X"/>, <see cref="Y"/>, <see cref="Width"/>, and <see cref="Height"/> set to zero.
        /// </summary>
        public static readonly Rectangle Empty = default;

        /// <summary>
        /// Initializes a new instance of <see cref="Rectangle"/>
        /// </summary>
        /// <param name="x">The x-coordinate of the instance</param>
        /// <param name="y">The y-coordinate of the instance</param>
        /// <param name="width">The width of the instance</param>
        /// <param name="height">The height of the instance</param>
        public Rectangle(int x, int y, int width, int height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }
    }
}
