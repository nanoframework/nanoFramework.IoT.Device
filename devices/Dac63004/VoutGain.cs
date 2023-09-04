////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Dac63004
{
    /// <summary>
    /// Selection of Vout gain.
    /// </summary>
    public enum VoutGain : byte
    {
        // the enum values correspond to the bit values in the register, MSB

        /// <summary>
        /// Using external reference on VREF pin. Multiply factor is 1x.
        /// </summary>
        ExternalRef_1x = 0x0,

        /// <summary>
        /// Using VDD as reference. Multiply factor is 1x.
        /// </summary>
        VddRef_1x = 0b0000_0100,

        /// <summary>
        /// Internal reference. Multiply factor is 1.5x.
        /// </summary>
        Internal_1_5x = 0b0000_1000,

        /// <summary>
        /// Internal reference. Multiply factor is 2x.
        /// </summary>
        Internal_2x = 0b0000_1100,

        /// <summary>
        /// Internal reference. Multiply factor is 3x.
        /// </summary>
        Internal_3x = 0b0001_0000,

        /// <summary>
        /// Internal reference. Multiply factor is 4x.
        /// </summary>
        Internal_4x = 0b0001_0100,
    }
}
