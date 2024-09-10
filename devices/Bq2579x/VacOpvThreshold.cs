////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Bq2579x
{
    /// <summary>
    /// VAC OVP thresholds.
    /// </summary>
    public enum VacOpvThreshold
    {
        /// <summary>
        /// Threshold is 26V.
        /// </summary>
        /// <remarks>
        /// Default value.
        /// </remarks>
        Opv_26V = 0b0000_0000,

        /// <summary>
        /// Threshold is 18V.
        /// </summary>
        Opv_18V = 0b0001_0000,

        /// <summary>
        /// Threshold is 12V.
        /// </summary>
        Opv_12V = 0b0010_0000,

        /// <summary>
        /// Threshold is 7V.
        /// </summary>
        Opv_7V = 0b0011_0000,
    }
}
