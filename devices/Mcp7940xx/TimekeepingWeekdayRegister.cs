// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp7940xx
{
    /// <summary>
    /// Timekeeping weekday register constants.
    /// </summary>
    internal enum TimekeepingWeekdayRegister
    {
        /// <summary>
        /// Shows if the oscillator is currently running.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <item>1 = Oscillator is enabled and running.</item>
        /// <item>0 = Oscillator has stopped or has been disabled.</item>
        /// <item>This bit is read only.</item>
        /// </list>
        /// </remarks>
        OscillatorRunning = 0b0010_0000,

        /// <summary>
        /// Shows if the device has experienced a power failure.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <item>1 = Primary power was lost. (This flag must be cleared by software.)</item>
        /// <item>0 = Primary power has not been lost.</item>
        /// </list>
        /// </remarks>
        PowerFailureStatus = 0b0001_0000,

        /// <summary>
        /// Shows if the device has experienced a power failure.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <item>1 = Battery backup input is enabled.</item>
        /// <item>0 = Battery backup input is disabled.</item>
        /// </list>
        /// </remarks>
        ExternalBatteryBackupEnabled = 0b0000_1000,

        /// <summary>
        /// Mask to seperate the bits pertaining to the weekday.
        /// </summary>
        WeekdayMask = 0b0000_0111
    }
}
