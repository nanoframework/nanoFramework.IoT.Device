////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Bq2579x
{
    /// <summary>
    /// ADC average initial value contro.
    /// </summary>
    public enum AdcInitialAverage : byte
    {
        /// <summary>
        /// Start average using the existing register value.
        /// </summary>
        ExistingValue = 0b0000_0000,

        /// <summary>
        /// Start average using a new ADC conversion.
        /// </summary>
        /// <remarks>This option is not available for IBAT discharge.</remarks>
        NewConversion = 0b0000_0100
    }
}
