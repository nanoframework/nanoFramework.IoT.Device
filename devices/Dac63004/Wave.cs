////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Dac63004
{
    /// <summary>
    /// Wave form option for function configuration.
    /// </summary>
    public enum Wave : ushort
    {
        // the enum values correspond to bits [10-8] in DAC-X-FUNC-CONFIG register 

        /// <summary>
        /// Triangular wave.
        /// </summary>
        Triangular = 0x0,

        /// <summary>
        /// Sawtooth wave.
        /// </summary>
        Sawtooth = 0b0000_0001_0000_0000,

        /// <summary>
        ///  Inverse sawtooth wave.
        /// </summary>
        InverseSawtooth = 0b0000_0010_0000_0000,

        /// <summary>
        /// Sine wave.
        /// </summary>
        Sine = 0b0000_0100_0000_0000,

        /// <summary>
        /// Disable function generation.
        /// </summary>
        Disable = 0b0000_0111_0000_0000
    }
}
