// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp7940xx
{
    /// <summary>
    /// Timekeeping second register constants.
    /// </summary>
    internal enum TimekeepingSecondRegister
    {
        /// <summary>
        /// Determines if the external oscillator input is active.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <item>1 = Oscillator input enabled.</item>
        /// <item>0 = Oscillator input disabled.</item>
        /// </list> 
        /// </remarks>
        OscillatorInputEnabled = 0b1000_0000,

        /// <summary>
        /// Mask to seperate the bits pertaining to the second.
        /// </summary>
        SecondMask = 0b0111_1111
    }
}
