// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp2101
{
    /// <summary>
    /// Charging status from Status2 register (0x01), bits [2:0].
    /// </summary>
    public enum ChargingStatus
    {
        /// <summary>Tri-charge state.</summary>
        TriCharge = 0,

        /// <summary>Pre-charge state.</summary>
        PreCharge = 1,

        /// <summary>Constant current charge state.</summary>
        ConstantCurrent = 2,

        /// <summary>Constant voltage charge state.</summary>
        ConstantVoltage = 3,

        /// <summary>Charge done.</summary>
        ChargeDone = 4,

        /// <summary>Not charging (stopped).</summary>
        NotCharging = 5,
    }
}
