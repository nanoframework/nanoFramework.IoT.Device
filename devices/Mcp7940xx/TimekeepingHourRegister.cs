// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp7940xx
{
    /// <summary>
    /// Timekeeping hour register constants.
    /// </summary>
    internal enum TimekeepingHourRegister
    {
        /// <summary>
        /// Determines 12 or 24 hour time format.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <item>1 = 12 hour format.</item>
        /// <item>0 = 24 hour format.</item>
        /// </list> 
        /// </remarks>
        TimeFormat = 0b0100_0000,

        /// <summary>
        /// Mask to seperate the bits pertaining to the hour.
        /// </summary>
        HourMask = 0b0011_1111
    }
}
