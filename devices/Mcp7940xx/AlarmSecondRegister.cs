// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp7940xx
{
    /// <summary>
    /// Alarm second register constants.
    /// </summary>
    /// <remarks>
    /// Used for both Alarm1 and Alarm2 registers.
    /// </remarks>
    internal enum AlarmSecondRegister
    {
        /// <summary>
        /// Mask to seperate the bits pertaining to the alarm second.
        /// </summary>
        SecondMask = 0b0111_1111
    }
}
