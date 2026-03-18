// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp2101
{
    /// <summary>
    /// Charge target voltage settings for CV_CHG_VOL_SET register (0x64), bits [2:0].
    /// </summary>
    public enum ChargeTargetVoltage
    {
        /// <summary>Target voltage 4.0V.</summary>
        Voltage4V0 = 1,

        /// <summary>Target voltage 4.1V.</summary>
        Voltage4V1 = 2,

        /// <summary>Target voltage 4.2V.</summary>
        Voltage4V2 = 3,

        /// <summary>Target voltage 4.35V.</summary>
        Voltage4V35 = 4,

        /// <summary>Target voltage 4.4V.</summary>
        Voltage4V4 = 5,
    }
}
