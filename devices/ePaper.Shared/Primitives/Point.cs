namespace Iot.Device.ePaper.Shared.Primitives
{
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
        public static Point Default = new(0, 0);

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

        public static bool InternalEquals(Point point, Point other)
            => point.X == other.X && point.Y == other.Y;
    }
}
