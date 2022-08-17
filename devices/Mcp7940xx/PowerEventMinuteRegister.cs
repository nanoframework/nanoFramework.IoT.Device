// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp7940xx
{
    /// <summary>
    /// Power event minute register constants.
    /// </summary>
    /// <remarks>
    /// Used for both power up and power down registers.
    /// </remarks>
    internal enum PowerEventMinuteRegister
    {
        /// <summary>
        /// Mask to seperate the bits pertaining to the minute.
        /// </summary>
        MinuteMask = 0b0111_1111,
    }
}
