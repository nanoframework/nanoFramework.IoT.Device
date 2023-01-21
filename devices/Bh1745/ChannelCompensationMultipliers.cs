// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Bh1745
{
    /// <summary>
    /// Channel compensation multipliers used to compensate the 4 color channels of the Bh1745.
    /// </summary>
    public class ChannelCompensationMultipliers
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelCompensationMultipliers" /> class.
        /// </summary>
        /// <param name="red">Red value.</param>
        /// <param name="green">Green value.</param>
        /// <param name="blue">Blue value.</param>
        /// <param name="clear">Clear value.</param>
        public ChannelCompensationMultipliers(double red, double green, double blue, double clear)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Clear = clear;
        }

        /// <summary>
        /// Gets red color value.
        /// </summary>
        public double Red { get; }

        /// <summary>
        /// Gets green color value.
        /// </summary>
        public double Green { get; }

        /// <summary>
        /// Gets blue color value.
        /// </summary>
        public double Blue { get; }

        /// <summary>
        /// Gets clear value.
        /// </summary>
        public double Clear { get; }
    }
}
