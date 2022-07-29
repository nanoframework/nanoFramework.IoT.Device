// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vl53L0X
{
    /// <summary>
    /// The measurement mode
    /// Continuous measurement is processed in the sensor and readings are
    /// more reliable than the Single measurement mode.
    /// </summary>
    public enum MeasurementMode
    {
        /// <summary>Continuous mode.</summary>
        Continuous = 0,

        /// <summary>Single measurement mode.</summary>
        Single
    }
}
