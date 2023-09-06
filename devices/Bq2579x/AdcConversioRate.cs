////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Bq2579x
{
    /// <summary>
    /// ADC conversion rate.
    /// </summary>
    public enum AdcConversioRate : byte
    {
        /// <summary>
        /// Continuous conversion.
        /// </summary>
        Continuous = 0b0000_0000,

        /// <summary>
        /// One-shot conversion.
        /// </summary>
        OneShot = 0b0100_0000
    }
}
