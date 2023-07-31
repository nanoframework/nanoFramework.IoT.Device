////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Bq2579x
{
    /// <summary>
    /// Thermal shutdown threshold for temperature control.
    /// </summary>
    public enum ThermalShutdownThreshold
    {
        /// <summary>
        /// Threshold value 150°C.
        /// </summary>
        Temp150C = 0,

        /// <summary>
        /// Threshold value 130°C.
        /// </summary>
        Temp130C = 1,

        /// <summary>
        /// Threshold value 120°C.
        /// </summary>
        Temp120C = 2,

        /// <summary>
        /// Threshold value 85°C.
        /// </summary>
        Temp85C = 3,
    }
}
