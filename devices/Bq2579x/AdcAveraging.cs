////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Bq2579x
{
    /// <summary>
    /// ADC average control.
    /// </summary>
    public enum AdcAveraging : byte
    {
        /// <summary>
        /// Single value.
        /// </summary>
        SingleValue = 0b0000_0000,

        /// <summary>
        /// Running average.
        /// </summary>
        /// <remarks>This option is not available for IBAT discharge.</remarks>
        RunningAverage = 0b0000_1000
    }
}
