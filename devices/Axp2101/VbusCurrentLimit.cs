// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp2101
{
    /// <summary>
    /// VBUS current input limit settings for INPUT_CUR_LIMIT_CTRL register (0x16), bits [2:0].
    /// </summary>
    public enum VbusCurrentLimit
    {
        /// <summary>VBUS current limit 100 mA.</summary>
        Current100mA = 0,

        /// <summary>VBUS current limit 500 mA.</summary>
        Current500mA = 1,

        /// <summary>VBUS current limit 900 mA.</summary>
        Current900mA = 2,

        /// <summary>VBUS current limit 1000 mA.</summary>
        Current1000mA = 3,

        /// <summary>VBUS current limit 1500 mA.</summary>
        Current1500mA = 4,

        /// <summary>VBUS current limit 2000 mA.</summary>
        Current2000mA = 5,
    }
}
