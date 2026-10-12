// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Max30100
{
    /// <summary>One raw optical sample from the MAX30100 FIFO.</summary>
    public struct Max30100Reading
    {
        /// <summary>Initializes a new instance of the <see cref="Max30100Reading"/> struct.</summary>
        /// <param name="infrared">The infrared ADC value.</param>
        /// <param name="red">The red ADC value.</param>
        public Max30100Reading(ushort infrared, ushort red)
        {
            Infrared = infrared;
            Red = red;
        }

        /// <summary>Gets the infrared ADC value.</summary>
        /// <value>The infrared ADC value.</value>
        public ushort Infrared { get; }

        /// <summary>Gets the red ADC value.</summary>
        /// <value>The red ADC value.</value>
        public ushort Red { get; }
    }
}
