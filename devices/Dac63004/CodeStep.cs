////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Dac63004
{
    /// <summary>
    /// Configuration for Code Step.
    /// </summary>
    public enum CodeStep : byte
    {
        // these correspond to the bit values [6-4] in DAC-X-FUNC-CONFIG register

        /// <summary>
        /// 1 LSB.
        /// </summary>
        Step1LSB = 0x0,

        /// <summary>
        /// 2 LSB.
        /// </summary>
        Step2LSB = 0b0001_0000,

        /// <summary>
        /// 3 LSB.
        /// </summary>
        Step3LSB = 0b0010_0000,

        /// <summary>
        /// 4 LSB.
        /// </summary>
        Step4LSB = 0b0011_0000,

        /// <summary>
        /// 6 LSB.
        /// </summary>
        Step6LSB = 0b0100_0000,

        /// <summary>
        /// 8 LSB.
        /// </summary>
        Step8LSB = 0b0101_0000,

        /// <summary>
        /// 16 LSB.
        /// </summary>
        Step16LSB = 0b0110_0000,

        /// <summary>
        /// 32 LSB.
        /// </summary>
        Step32LSB = 0b0111_0000
    }
}
