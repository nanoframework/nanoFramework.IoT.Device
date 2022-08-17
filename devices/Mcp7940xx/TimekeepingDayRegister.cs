// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp7940xx
{
    /// <summary>
    /// Timekeeping day register constants.
    /// </summary>
    internal enum TimekeepingDayRegister
    {
        /// <summary>
        /// Mask to seperate the bits pertaining to the day.
        /// </summary>
        DayMask = 0b0111_1111
    }
}
