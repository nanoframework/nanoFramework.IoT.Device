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
        /// Constructor for ChannelCompensationMultipliers
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <param name="clear"></param>
        public ChannelCompensationMultipliers(double red, double green, double blue, double clear)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Clear = clear;
        }

        /// <summary>
        /// Red
        /// </summary>
        public double Red { get; }

        /// <summary>
        /// Green
        /// </summary>
        public double Green { get; }

        /// <summary>
        /// Blue
        /// </summary>
        public double Blue { get; }

        /// <summary>
        /// Clear
        /// </summary>
        public double Clear { get; }
    }
}
