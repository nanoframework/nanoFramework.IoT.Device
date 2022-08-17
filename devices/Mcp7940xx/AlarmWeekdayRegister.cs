// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp7940xx
{
    /// <summary>
    /// Alarm day-of-the-week register constants.
    /// </summary>
    /// <remarks>
    /// Used for both Alarm1 and Alarm2 registers.
    /// </remarks>
    internal enum AlarmWeekdayRegister : byte
    {
        /// <summary>
        /// Determines polarity of the MFP pin when in Alarm Interrupt Output mode.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <item>1 = MFP is a logic high when alarm is asserted.</item>
        /// <item>0 = MFP is a logic low when alarm is asserted.</item>
        /// </list>
        /// </remarks>
        AlarmInterruptPolarity = 0b10000000,

        /// <summary>
        /// Mask to seperate the bits pertaining to the alarm match mode.
        /// </summary>
        /// <remarks>
        /// Must contain a value defined in <see cref="AlarmMatchMode" />.
        /// </remarks>
        AlarmMatchModeMask = 0b01110000,

        /// <summary>
        /// Indicates if the alarm interrupt has been triggered.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <item>1 = Alarm has been triggered. (This flag must be cleared by software.)</item>
        /// <item>0 = Alarm has not been triggered.</item>
        /// </list>
        /// </remarks>
        AlarmInterrupt = 0b00001000,

        /// <summary>
        /// Mask to seperate the bits pertaining to the alarm day-of-the-week.
        /// </summary>
        /// <remarks>
        /// Contains a value from 1 to 7. The representation is user-defined.
        /// </remarks>
        DayOfWeekMask = 0b00000111
    }
}
