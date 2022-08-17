// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp7940xx
{
    /// <summary>
    /// Power event month register constants.
    /// </summary>
    /// <remarks>
    /// Used for both power up and power down registers.
    /// </remarks>
    internal enum PowerEventMonthRegister
    {
        /// <summary>
        /// Mask to seperate the bits pertaining to the month.
        /// </summary>
        MonthMask = 0b0001_1111,

        /// <summary>
        /// Mask to seperate the bits pertaining to the weekday.
        /// </summary>
        WeekdayMask = 0b1110_0000
    }
}
