// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Amg88xx
{
    /// <summary>
    /// Simple Point class.
    /// </summary>
    public class Point
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Point" /> class.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Gets or sets x coordinate.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Gets or sets y coordinate.
        /// </summary>
        public int Y { get; set; }
    }
}
