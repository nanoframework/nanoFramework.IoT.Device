////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Bq2579x
{
    /// <summary>
    /// Battery discharging current regulation in OTG mode.
    /// </summary>
    public enum BatteryDischargeCurrent : byte
    {
        /// <summary>
        /// Current 3A.
        /// </summary>
        Current_3A = 0b0000_0000,

        /// <summary>
        /// Current 4A.
        /// </summary>
        Current_4A = 0b0000_1000,

        /// <summary>
        /// Current 5A.
        /// </summary>
        Current_5A = 0b0001_0000,

        /// <summary>
        /// Disabled.
        /// </summary>
        Disable = 0b0001_1000
    }
}
