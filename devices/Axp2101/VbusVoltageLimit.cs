// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp2101
{
    /// <summary>
    /// VBUS voltage input limit settings for INPUT_VOL_LIMIT_CTRL register (0x15), bits [3:0].
    /// </summary>
    public enum VbusVoltageLimit
    {
        /// <summary>VBUS voltage limit 3.88V.</summary>
        Voltage3V88 = 0,

        /// <summary>VBUS voltage limit 3.96V.</summary>
        Voltage3V96 = 1,

        /// <summary>VBUS voltage limit 4.04V.</summary>
        Voltage4V04 = 2,

        /// <summary>VBUS voltage limit 4.12V.</summary>
        Voltage4V12 = 3,

        /// <summary>VBUS voltage limit 4.20V.</summary>
        Voltage4V20 = 4,

        /// <summary>VBUS voltage limit 4.28V.</summary>
        Voltage4V28 = 5,

        /// <summary>VBUS voltage limit 4.36V.</summary>
        Voltage4V36 = 6,

        /// <summary>VBUS voltage limit 4.44V.</summary>
        Voltage4V44 = 7,

        /// <summary>VBUS voltage limit 4.52V.</summary>
        Voltage4V52 = 8,

        /// <summary>VBUS voltage limit 4.60V.</summary>
        Voltage4V60 = 9,

        /// <summary>VBUS voltage limit 4.68V.</summary>
        Voltage4V68 = 10,

        /// <summary>VBUS voltage limit 4.76V.</summary>
        Voltage4V76 = 11,

        /// <summary>VBUS voltage limit 4.84V.</summary>
        Voltage4V84 = 12,

        /// <summary>VBUS voltage limit 4.92V.</summary>
        Voltage4V92 = 13,

        /// <summary>VBUS voltage limit 5.00V.</summary>
        Voltage5V00 = 14,

        /// <summary>VBUS voltage limit 5.08V.</summary>
        Voltage5V08 = 15,
    }
}
