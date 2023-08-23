////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Bq2579x
{
    /// <summary>
    /// VT3 comparator voltage falling thresholds as a percentage of REGN.
    /// </summary>
    /// <remarks>
    /// The corresponding temperature in the brackets is achieved when a 103AT NTC thermistor is used, RT1=5.24kΩ and RT2=30.31kΩ.
    /// </remarks>
    public enum CompVoltageFallingThreshold
    {
        /// <summary>
        /// Threshold 48.4% (40°C).
        /// </summary>
        VregN48_4 = 0b0000,

        /// <summary>
        /// Threshold 44.8% (45°C).
        /// </summary>
        /// <remarks>
        /// Default value.
        /// </remarks>
        VregN44_8 = 0b0001,

        /// <summary>
        /// Threshold 41.2% (50°C).
        /// </summary>
        VregN41_2 = 0b0010,

        /// <summary>
        /// Threshold 37.7% (55°C).
        /// </summary>
        VregN37_7 = 0b0011,
    }
}
