//
// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
//

namespace Iot.Device.CharacterLcd
{
    /// <summary>
    /// Represents a single point on a plane.
    /// </summary>
    public struct Point
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
        /// An empty representation of a <see cref="Point"/> with <see cref="X"/> and <see cref="Y"/> set to zero.
        /// </summary>
        public static readonly Point Empty = default;

        /// <summary>
        /// Initializes a new instance of <see cref="Rectangle"/>
        /// </summary>
        /// <param name="x">The x-coordinate of the instance</param>
        /// <param name="y">The y-coordinate of the instance</param>
        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}
