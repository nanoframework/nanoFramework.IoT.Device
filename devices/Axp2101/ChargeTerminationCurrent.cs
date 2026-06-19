// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp2101
{
    /// <summary>
    /// Charge termination current settings for ITERM_CHG_SET_CTRL register (0x63), bits [3:0].
    /// Each step is 25 mA.
    /// </summary>
    public enum ChargeTerminationCurrent
    {
        /// <summary>Termination current 0 mA.</summary>
        Current0mA = 0,

        /// <summary>Termination current 25 mA.</summary>
        Current25mA = 1,

        /// <summary>Termination current 50 mA.</summary>
        Current50mA = 2,

        /// <summary>Termination current 75 mA.</summary>
        Current75mA = 3,

        /// <summary>Termination current 100 mA.</summary>
        Current100mA = 4,

        /// <summary>Termination current 125 mA.</summary>
        Current125mA = 5,

        /// <summary>Termination current 150 mA.</summary>
        Current150mA = 6,

        /// <summary>Termination current 175 mA.</summary>
        Current175mA = 7,

        /// <summary>Termination current 200 mA.</summary>
        Current200mA = 8,
    }
}
