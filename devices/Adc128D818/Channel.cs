// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Adc128D818
{
    /// <summary>
    /// ADC128D818 channel.
    /// </summary>
    /// <remarks>
    /// Mind the <see cref="Mode"/> when using a channel, as not all channels are available in all modes.
    /// </remarks>
    public enum Channel : byte
    {
        /// <summary>
        /// Channel IN0, single-ended reading.
        /// </summary>
        In0 = 0,

        /// <summary>
        /// Channel IN1, single-ended reading.
        /// </summary>
        In1 = 1,

        /// <summary>
        /// Channel IN2, single-ended reading.
        /// </summary>
        In2 = 2,

        /// <summary>
        /// Channel IN3, single-ended reading.
        /// </summary>
        In3 = 3,

        /// <summary>
        /// Channel IN4, single-ended reading.
        /// </summary>
        In4 = 4,

        /// <summary>
        /// Channel IN5, single-ended reading.
        /// </summary>
        In5 = 5,

        /// <summary>
        /// Channel IN6, single-ended reading.
        /// </summary>
        In6 = 6,

        /// <summary>
        /// Channel IN7, single-ended reading.
        /// </summary>
        In7 = 7,

        /// <summary>
        /// Local temperature sensor reading.
        /// </summary>
        Temperature = 8,

        /// <summary>
        /// Channel IN0+ amd Channel IN1-, differential reading.
        /// </summary>
        In0_In1_Differential = 9,

        /// <summary>
        /// Channel IN3+ amd Channel IN2-, differential reading.
        /// </summary>
        In3_In2_Differential = 10,

        /// <summary>
        /// Channel IN4+ amd Channel IN5-, differential reading.
        /// </summary>
        In4_In5_Differential = 11,

        /// <summary>
        /// Channel IN7+ amd Channel IN6-, differential reading.
        /// </summary>
        In7_In6_Differential = 12
    }
}
