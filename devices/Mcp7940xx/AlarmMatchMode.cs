// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp7940xx
{
    /// <summary>
    /// Available modes for determining when alarm should trigger.
    /// </summary>
    public enum AlarmMatchMode
    {
        /// <summary>
        /// Alarm triggers when the second matches.
        /// </summary>
        Second = 0b0000_0000,

        /// <summary>
        /// Alarm triggers when the minute matches.
        /// </summary>
        Minute = 0b0001_0000,

        /// <summary>
        /// Alarm triggers when the hour matches.
        /// </summary>
        Hour = 0b0010_0000,

        /// <summary>
        /// Alarm triggers when the day-of-the-week matches.
        /// </summary>
        DayOfWeek = 0b0011_0000,

        /// <summary>
        /// Alarm triggers when the day-of-the-month matches.
        /// </summary>
        Day = 0b0100_0000,

        /// <summary>
        /// Alarm triggers when the second, minute, hour, day-of-the-week, day-of-the-month and month matches.
        /// </summary>
        Full = 0b0111_0000
    }
}
