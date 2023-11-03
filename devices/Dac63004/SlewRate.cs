////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Dac63004
{
    /// <summary>
    /// Configuration for Slew Rate.
    /// </summary>
    public enum SlewRate : byte
    {
        // these correspond to the bit values [3-0] in DAC-X-FUNC-CONFIG register

        /// <summary>
        /// No slew rate.
        /// </summary>
        NoSlewRate = 0,

        /// <summary>
        /// Time period is 4µs.
        /// </summary>
        Time4us = 0b0000_0001,

        /// <summary>
        /// Time period is 8µs.
        /// </summary>
        Time8us = 0b0000_0010,

        /// <summary>
        /// Time period is 12µs.
        /// </summary>
        Time12us = 0b0000_0011,

        /// <summary>
        /// Time period is 18µs.
        /// </summary>
        Time18us = 0b0000_0100,

        /// <summary>
        /// Time period is 27µs.
        /// </summary>
        Time27us = 0b0000_0101,

        /// <summary>
        /// Time period is 40.5µs.
        /// </summary>
        Time40_5us = 0b0000_0110,

        /// <summary>
        /// Time period is 60.75µs.
        /// </summary>
        Time60_75us = 0b0000_0111,

        /// <summary>
        /// Time period is 91.13µs.
        /// </summary>
        Time91_13us = 0b0000_1000,

        /// <summary>
        /// Time period is 136.69µs.
        /// </summary>
        Time136_69us = 0b0000_1001,

        /// <summary>
        /// Time period is 239.2µs.
        /// </summary>
        Time239_2us = 0b0000_1010,

        /// <summary>
        /// Time period is 418.61µs.
        /// </summary>
        Time418_61us = 0b0000_1011,

        /// <summary>
        /// Time period is 732.56µs.
        /// </summary>
        Time732_56us = 0b0000_1100,

        /// <summary>
        /// Time period is 1281.98µs.
        /// </summary>
        Time1281_98us = 0b0000_1101,

        /// <summary>
        /// Time period is 2563.96µs.
        /// </summary>
        Time2563_96us = 0b0000_1110,

        /// <summary>
        /// Time period is 5127.92µs.
        /// </summary>
        Time5127_92us = 0b0000_1111
    }
}
