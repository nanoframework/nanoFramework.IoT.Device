// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp7940xx
{
    /// <summary>
    /// Timekeeping month register constants.
    /// </summary>
    internal enum TimekeepingMonthRegister
    {
        /// <summary>
        /// Determines if the current year is a leap year.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <item>1 = Year is a leap year.</item>
        /// <item>0 = Year is not a leap year.</item>
        /// </list> 
        /// </remarks>
        IsLeapYear = 0b0010_0000,

        /// <summary>
        /// Mask to seperate the bits pertaining to the month.
        /// </summary>
        MonthMask = 0b0001_1111
    }
}
