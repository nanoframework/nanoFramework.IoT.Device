////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Dac63004
{
    /// <summary>
    /// Selection of functional mode of the DAC channel.
    /// </summary>
    public enum Mode : byte
    {
        /// <summary>
        /// Voltage-Output Mode.
        /// </summary>
        VoltageOutput,

        /// <summary>
        /// Current output mode.
        /// </summary>
        CurrentOutput,

        /// <summary>
        /// Comparator mode.
        /// </summary>
        Comparator
    }
}
