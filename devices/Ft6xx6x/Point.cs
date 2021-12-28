// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ft6xx6x
{
    /// <summary>
    /// A touch point
    /// </summary>
    public struct Point
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
        /// Gets or sets the touche type.
        /// </summary>
        public byte TouchId { get; set; }

        /// <summary>
        /// Gets of sets the weight.
        /// </summary>
        public byte Weigth { get; set; }

        /// <summary>
        /// Gets or sets miscelaneous.
        /// </summary>
        public byte Miscelaneous { get; set; }

        /// <summary>
        /// Gets or sets the event type.
        /// </summary>
        public Event Event { get; set; }
    }
}
