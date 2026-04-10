// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp2101
{
    /// <summary>
    /// Pre-charge current settings for IPRECHG_SET register (0x61), bits [3:0].
    /// Each step is 25 mA.
    /// </summary>
    public enum PrechargeCurrent
    {
        /// <summary>Pre-charge current 0 mA.</summary>
        Current0mA = 0,

        /// <summary>Pre-charge current 25 mA.</summary>
        Current25mA = 1,

        /// <summary>Pre-charge current 50 mA.</summary>
        Current50mA = 2,

        /// <summary>Pre-charge current 75 mA.</summary>
        Current75mA = 3,

        /// <summary>Pre-charge current 100 mA.</summary>
        Current100mA = 4,

        /// <summary>Pre-charge current 125 mA.</summary>
        Current125mA = 5,

        /// <summary>Pre-charge current 150 mA.</summary>
        Current150mA = 6,

        /// <summary>Pre-charge current 175 mA.</summary>
        Current175mA = 7,

        /// <summary>Pre-charge current 200 mA.</summary>
        Current200mA = 8,
    }
}
