// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Chsc6540
{
    /// <summary>
    /// A double point
    /// </summary>
    public class DoublePoints
    {
        /// <summary>
        /// Gets or sets the first point.
        /// </summary>
        public Point Point1 { get; set; }

        /// <summary>
        /// Gets or sets the second point.
        /// </summary>
        public Point Point2 { get; set; }

        /// <summary>
        /// Constructor for <see cref="DoublePoints"/> class.
        /// </summary>
        public DoublePoints()
        {
            Point1 = new Point();
            Point2 = new Point();
        }
    }
}
