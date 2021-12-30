// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.Runtime.Events;
using System;

namespace Iot.Device.Ft6xx6x.Samples
{
    /// <summary>
    /// Touch event arguments.
    /// </summary>
    public class TouchEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the event category.
        /// </summary>
        public EventCategory Category { get; set; }

        /// <summary>
        /// Gets or sets the sub category.
        /// </summary>
        public int SubCategory { get; set; }

        /// <summary>
        /// Gets or sets the first point X coordinate.
        /// </summary>
        public int X1 { get; set; }

        /// <summary>
        /// Gets or sets the the first point Y coordinate.
        /// </summary>
        public int Y1 { get; set; }

        /// <summary>
        /// Gets or sets the second point X coordinate.
        /// </summary>
        public int X2 { get; set; }

        /// <summary>
        /// Gets or sets the second point Y coortinate.
        /// </summary>
        public int Y2 { get; set; }
    }
}
