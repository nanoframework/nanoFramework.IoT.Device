////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Bq2579x
{
    /// <summary>
    /// VT2 comparator voltage rising thresholds as a percentage of REGN.
    /// </summary>
    /// <remarks>
    /// The corresponding temperature in the brackets is achieved when a 103AT NTC thermistor is used, RT1=5.24kΩ and RT2=30.31kΩ.
    /// </remarks>
    public enum CompVoltageRisingThreshold
    {
        /// <summary>
        /// Threshold 71.1% (5°C).
        /// </summary>
        VregN71_1 = 0b0000,

        /// <summary>
        /// Threshold 68.4% (default) (10°C).
        /// </summary>
        /// <remarks>
        /// Default value.
        /// </remarks>
        VregN68_4 = 0b0001,

        /// <summary>
        /// Threshold 65.5% (15°C).
        /// </summary>
        VregN65_5 = 0b0010,

        /// <summary>
        /// Threshold 62.4% (20°C).
        /// </summary>
        VregN62_4 = 0b0011,
    }
}
