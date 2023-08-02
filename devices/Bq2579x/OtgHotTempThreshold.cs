////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Bq2579x
{
    /// <summary>
    /// OTG mode TS HOT temperature threshold.
    /// </summary>
    public enum OtgHotTempThreshold
    {
        /// <summary>
        /// Threshold at 55°C.
        /// </summary>
        Temp55 = 0b0000,

        /// <summary>
        /// Threshold 60°C.
        /// </summary>
        /// <remarks>
        /// Default value.
        /// </remarks>
        Temp60 = 0b0001,

        /// <summary>
        /// Threshold at 65°C.
        /// </summary>
        Temp65 = 0b0010,

        /// <summary>
        /// Disable.
        /// </summary>
        Disable = 0b0011
    }
}
