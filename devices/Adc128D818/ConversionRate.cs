// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Adc128D818
{
    /// <summary>
    /// ADC128D818 conversion rate.
    /// </summary>
    public enum ConversionRate : byte
    {
        /// <summary>
        /// Low Power Conversion Mode.
        /// </summary>
        LowPower = 0,

        /// <summary>
        /// Continuous Conversion Mode.
        /// </summary>
        Continuous = 1,
    }
}
