////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Dac63004
{
    /// <summary>
    /// Selection of DAC channel.
    /// </summary>
    public enum Channel : byte
    {
        /// <summary>
        /// DAC channel 0.
        /// </summary>
        Channel0 = 0x00,

        /// <summary>
        /// DAC channel 1.
        /// </summary>
        Channel1 = 0x01,

        /// <summary>
        /// DAC channel 2.
        /// </summary>
        Channel2 = 0x02,

        /// <summary>
        /// DAC channel 3.
        /// </summary>
        Channel3 = 0x03,
    }
}
