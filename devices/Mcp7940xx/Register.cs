// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp7940xx
{
    /// <summary>
    /// Mcp7940xx real time clock/calendar register map.
    /// </summary>
    internal enum Register : byte
    {
        /// <summary>
        /// Holds the second component of the clock.
        /// </summary>
        TimekeepingSecond = 0x00,

        /// <summary>
        /// Holds the minute component of the clock.
        /// </summary>    
        TimekeepingMinute = 0x01,

        /// <summary>
        /// Holds the hour component of the clock.
        /// </summary>
        TimekeepingHour = 0x02,

        /// <summary>
        /// Holds the day-of-the-week component of the clock.
        /// </summary>
        TimekeepingWeekday = 0x03,

        /// <summary>
        /// Holds the day-of-the-month component of the clock.
        /// </summary>
        TimekeepingDay = 0x04,

        /// <summary>
        /// Holds the month component of the clock.
        /// </summary>
        TimekeepingMonth = 0x05,

        /// <summary>
        /// Holds the year component of the clock.
        /// </summary>
        TimekeepingYear = 0x06,

        /// <summary>
        /// Holds device control flags.
        /// </summary>
        Control = 0x07,

        /// <summary>
        /// Holds the oscillator trimming amount.
        /// </summary>
        OscillatorTrimming = 0x08,

        /// <summary>
        /// Protected EEPROM Unlock Register.
        /// </summary>
        EepromUnlock = 0x09,

        /// <summary>
        /// Holds the second component of Alarm 1.
        /// </summary>
        Alarm1Second = 0x0A,

        /// <summary>
        /// Holds the minute component of Alarm 1.
        /// </summary>
        Alarm1Minute = 0x0B,

        /// <summary>
        /// Holds the hour component of Alarm 1.
        /// </summary>
        Alarm1Hour = 0x0C,

        /// <summary>
        /// Holds the day-of-the-week component of Alarm 1.
        /// </summary>
        Alarm1Weekday = 0x0D,

        /// <summary>
        /// Holds the day-of-the-month component of Alarm 1.
        /// </summary>
        Alarm1Day = 0x0E,

        /// <summary>
        /// Holds the month component of Alarm 1.
        /// </summary>
        Alarm1Month = 0x0F,

        /// <summary>
        /// Holds the second component of Alarm 2.
        /// </summary>
        Alarm2Second = 0x11,

        /// <summary>
        /// Holds the minute component of Alarm 2.
        /// </summary>
        Alarm2Minute = 0x12,

        /// <summary>
        /// Holds the hour component of Alarm 2.
        /// </summary>
        Alarm2Hour = 0x13,

        /// <summary>
        /// Holds the day-of-the-week component of Alarm 2.
        /// </summary>
        Alarm2Weekday = 0x14,

        /// <summary>
        /// Holds the day-of-the-month component of Alarm 2.
        /// </summary>
        Alarm2Day = 0x15,

        /// <summary>
        /// Holds the month component of Alarm 2.
        /// </summary>
        Alarm2Month = 0x16,

        /// <summary>
        /// Holds the minute component of the last power down event.
        /// </summary>
        PowerDownMinute = 0x18,

        /// <summary>
        /// Holds the hour component of the last power down event.
        /// </summary>
        PowerDownHour = 0x19,

        /// <summary>
        /// Holds the day-of-the-week component of the last power down event.
        /// </summary>
        PowerDownDay = 0x1A,

        /// <summary>
        /// Holds the month component of the last power down event.
        /// </summary>
        PowerDownMonth = 0x1B,

        /// <summary>
        /// Holds the minute component of the last power up event.
        /// </summary>
        PowerUpMinute = 0x1C,

        /// <summary>
        /// Holds the hour component of the last power up event.
        /// </summary>
        PowerUpHour = 0x1D,

        /// <summary>
        /// Holds the day-of-the-week component of the last power up event.
        /// </summary>
        PowerUpDay = 0x1E,

        /// <summary>
        /// Holds the month component of the last power up event.
        /// </summary>
        PowerUpMonth = 0x1F
    }
}
