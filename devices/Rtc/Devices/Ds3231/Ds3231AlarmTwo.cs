// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Rtc
{
    /// <summary>
    /// Represents alarm 2 on the DS3231.
    /// </summary>
    public class Ds3231AlarmTwo
    {
        /// <summary>
        /// Gets or sets day of month or day of week of the alarm. Which one it is depends on the match mode.
        /// </summary>
        public int DayOfMonthOrWeek { get; set; }

        /// <summary>
        /// Gets or sets the time the alarm, Hour and Minute are used.
        /// </summary>
        public TimeSpan AlarmTime { get; set; }

        /// <summary>
        /// Gets or sets mode to use to determine when to trigger the alarm.
        /// </summary>
        public Ds3231AlarmTwoMatchMode MatchMode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Ds3231AlarmTwo" /> class.
        /// </summary>
        /// <param name="dayOfMonthOrWeek">Day of month or day of week of the alarm. Which one it is depends on the match mode.</param>
        /// <param name="alarmTime">The time the alarm, Hour and Minute are used.</param>
        /// <param name="matchMode">Mode to use to determine when to trigger the alarm.</param>
        public Ds3231AlarmTwo(int dayOfMonthOrWeek, TimeSpan alarmTime, Ds3231AlarmTwoMatchMode matchMode)
        {
            DayOfMonthOrWeek = dayOfMonthOrWeek;
            AlarmTime = alarmTime;
            MatchMode = matchMode;
        }
    }
}
