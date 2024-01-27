// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace IoT.Device.AtModem.DTOs
{
    /// <summary>
    /// Represents the battery charge status.
    /// </summary>
    public enum BatteryChargeStatus : byte
    {
        /// <summary>
        /// Powered by battery.
        /// </summary>
        PoweredByBattery = 0,

        /// <summary>
        /// Charging.
        /// </summary>
        Charging = 1,

        /// <summary>
        /// Charging finished.
        /// </summary>
        ChargingFinished = 2,

        /// <summary>
        /// Power fault.
        /// </summary>
        PowerFault = 3,
    }
}
