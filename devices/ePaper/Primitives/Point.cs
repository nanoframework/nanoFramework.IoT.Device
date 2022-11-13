// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.EPaper.Primitives
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
            X = x;
            Y = y;
        }

        /// <summary>
        /// The start point (X = 0, Y = 0).
        /// </summary>
        public static Point Zero = new Point(0, 0);

        /// <summary>
        /// Checks if the specified points are equivalent based on their <see cref="X"/> and <see cref="Y"/> values.
        /// </summary>
        /// <param name="point">The first to <see cref="Point"/> to check.</param>
        /// <param name="other">The other <see cref="Point"/> to check.</param>
        /// <returns>True if both instances are the same position.</returns>
        public static bool operator ==(Point point, Point other)
            => InternalEquals(point, other);

        /// <summary>
        /// Checks if the specified points are equivalent based on their <see cref="X"/> and <see cref="Y"/> values.
        /// </summary>
        /// <param name="point">The first to <see cref="Point"/> to check.</param>
        /// <param name="other">The other <see cref="Point"/> to check.</param>
        /// <returns>True if one <see cref="Color"/> refers to a different position than the other.</returns>
        public static bool operator !=(Point point, Point other)
            => !InternalEquals(point, other);

        /// <summary>
        /// Sums the <see cref="X"/> and <see cref="Y"/> values of the specified points.
        /// </summary>
        /// <param name="point">The first point.</param>
        /// <param name="other">The other point.</param>
        /// <returns>A new <see cref="Point"/> with the sum of <see cref="X"/> and <see cref="Y"/> values of the specified points.</returns>
        public static Point operator +(Point point, Point other)
            => new Point(point.X + other.X, point.Y + other.Y);

        /// <summary>
        /// Deducts the <see cref="X"/> and <see cref="Y"/> values of the specified points.
        /// </summary>
        /// <param name="point">The first point.</param>
        /// <param name="other">The other point.</param>
        /// <returns>A new <see cref="Point"/> with the deduction of <see cref="X"/> and <see cref="Y"/> values of the specified points.</returns>
        public static Point operator -(Point point, Point other)
            => new Point(point.X - other.X, point.Y - other.Y);

        /// <inheritdoc/>>
        public override bool Equals(object obj)
        {
            return obj is Point other && InternalEquals(this, other);
        }

        /// <inheritdoc/>>
        public override int GetHashCode()
        {
            unchecked
            {
                return X.GetHashCode() ^ Y.GetHashCode();
            }
        }

        private static bool InternalEquals(Point point, Point other)
        {
            return point.X == other.X && point.Y == other.Y;
        }
    }
}
