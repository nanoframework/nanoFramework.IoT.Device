// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Chsc6540
{
    /// <summary>
    /// A touch point
    /// </summary>
    public class Point
    {
        /// <summary>
        /// Gets or sets X
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Gets or sets Y
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Event"/> type.
        /// </summary>
        public Event Event { get; set; }
    }
}
