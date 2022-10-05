// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Rtc
{
    /// <summary>
    /// Represents alarm 1 on the DS3231.
    /// </summary>
    public class Ds3231AlarmOne
    {
        /// <summary>
        /// Gets or sets day of month or day of week of the alarm. Which one it is depends on the match mode.
        /// </summary>
        public int DayOfMonthOrWeek { get; set; }

        /// <summary>
        /// Gets or sets the time the alarm, Hour, Minute and Second are used.
        /// </summary>
        public TimeSpan AlarmTime { get; set; }

        /// <summary>
        /// Gets or sets mode to use to determine when to trigger the alarm.
        /// </summary>
        public Ds3231AlarmOneMatchMode MatchMode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Ds3231AlarmOne" /> class.
        /// </summary>
        /// <param name="dayOfMonthOrWeek">Day of month or day of week of the alarm. Which one it is depends on the match mode.</param>
        /// <param name="alarmTime">Time of the alarm.</param>
        /// <param name="matchMode">Mode to use to determine when to trigger the alarm.</param>
        public Ds3231AlarmOne(int dayOfMonthOrWeek, TimeSpan alarmTime, Ds3231AlarmOneMatchMode matchMode)
        {
            DayOfMonthOrWeek = dayOfMonthOrWeek;
            AlarmTime = alarmTime;
            MatchMode = matchMode;
        }
    }
}
