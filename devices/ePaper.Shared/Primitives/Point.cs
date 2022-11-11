// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.ePaper.Shared.Primitives
{
    /// <summary>
    /// A point data structure to represent a position on a plane.
    /// </summary>
    public struct Point
    {
        /// <summary>
        /// Gets the X position for this point.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Gets the Y position for this point.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Point"/> struct.
        /// </summary>
        /// <param name="x">The X position.</param>
        /// <param name="y">The Y position.</param>
        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// The start point (X = 0, Y = 0).
        /// </summary>
        public static Point Zero = new(0, 0);

        /// <inheritdoc/>>
        public override bool Equals(object obj)
            => obj is Point other && InternalEquals(this, other);

        /// <inheritdoc/>>
        public override int GetHashCode()
        {
            unchecked
            {
                return this.X.GetHashCode() ^ this.Y.GetHashCode();
            }
        }

        public static bool operator ==(Point point, Point other)
            => InternalEquals(point, other);

        public static bool operator !=(Point point, Point other)
            => !InternalEquals(point, other);

        public static Point operator +(Point point, Point other)
            => new Point(point.X + other.X, point.Y + other.Y);

        public static Point operator -(Point point, Point other)
            => new Point(point.X - other.X, point.Y - other.Y);


        private static bool InternalEquals(Point point, Point other)
            => point.X == other.X && point.Y == other.Y;
    }
}
