// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp2101
{
    /// <summary>
    /// Constant charge current settings for ICC_CHG_SET register (0x62), bits [4:0].
    /// </summary>
    public enum ChargingCurrent
    {
        /// <summary>Current 0 mA (charging disabled).</summary>
        Current0mA = 0,

        /// <summary>Current 25 mA.</summary>
        Current25mA = 1,

        /// <summary>Current 50 mA.</summary>
        Current50mA = 2,

        /// <summary>Current 75 mA.</summary>
        Current75mA = 3,

        /// <summary>Current 100 mA.</summary>
        Current100mA = 4,

        /// <summary>Current 125 mA.</summary>
        Current125mA = 5,

        /// <summary>Current 150 mA.</summary>
        Current150mA = 6,

        /// <summary>Current 175 mA.</summary>
        Current175mA = 7,

        /// <summary>Current 200 mA.</summary>
        Current200mA = 8,

        /// <summary>Current 300 mA.</summary>
        Current300mA = 9,

        /// <summary>Current 400 mA.</summary>
        Current400mA = 10,

        /// <summary>Current 500 mA.</summary>
        Current500mA = 11,

        /// <summary>Current 600 mA.</summary>
        Current600mA = 12,

        /// <summary>Current 700 mA.</summary>
        Current700mA = 13,

        /// <summary>Current 800 mA.</summary>
        Current800mA = 14,

        /// <summary>Current 900 mA.</summary>
        Current900mA = 15,

        /// <summary>Current 1000 mA.</summary>
        Current1000mA = 16,
    }
}
