////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Bq2579x
{
    /// <summary>
    /// OTG mode TS COLD temperature threshold.
    /// </summary>
    public enum OtgColdTempThreshold
    {
        /// <summary>
        /// Threshold at -10°C.
        /// </summary>
        Temp10 = 0b0000,

        /// <summary>
        /// Threshold at -20°C.
        /// </summary>
        Temp20 = 0b0001
    }
}
