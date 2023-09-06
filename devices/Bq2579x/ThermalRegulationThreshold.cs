////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Bq2579x
{
    /// <summary>
    /// Thermal regulation threshold for temperature control.
    /// </summary>
    public enum ThermalRegulationThreshold
    {
        /// <summary>
        /// Threshold value 60°C.
        /// </summary>
        Temp60C = 0,

        /// <summary>
        /// Threshold value 80°C.
        /// </summary>
        Temp80C = 1,

        /// <summary>
        /// Threshold value 100°C.
        /// </summary>
        Temp100C = 2,

        /// <summary>
        /// Threshold value 120°C.
        /// </summary>
        Temp120C = 3,
    }
}
