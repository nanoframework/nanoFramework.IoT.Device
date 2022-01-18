using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.BuildHat
{
    /// <summary>
    /// A Point class.
    /// </summary>
    public class Point
    {
        /// <summary>
        /// Creates a Point.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Gets or sets X coordinate.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Gets or sets Y coordinate.
        /// </summary>
        public int Y { get; set; }
    }
}
