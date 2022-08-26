// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ds1621
{
    /// <summary>
    /// Defines the devices response to a measure temperature command.
    /// </summary>
    public enum MeasurementMode : byte
    {
        /// <summary>
        /// The device will continuously perform temperature measurements.
        /// </summary>
        Continuous,

        /// <summary>
        /// The device will perform a single temperature measurement.
        /// </summary>
        Single
    }
}
