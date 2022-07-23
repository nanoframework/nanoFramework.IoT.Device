// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Rtc
{
    internal enum PCF85263RtcRegisters : byte
    {
        //RTC time and date registers
        RTC_SECOND_100TH_ADDR = 0x00,
        RTC_SECOND_ADDR = 0x01,
        RTC_MINUTE_ADDR = 0x02,
        RTC_HOUR_ADDR = 0x03,
        RTC_DAY_ADDR = 0x04,
        RTC_WEEK_ADDR = 0x05,
        RTC_MONTH_ADDR = 0x06,
        RTC_YEAR_ADDR = 0x07,

        //RTC alarm1
        RTC_ALARM1_SECOND_ADDR = 0x08,
        RTC_ALARM1_MINUTE_ADDR = 0x09,
        RTC_ALARM1_HOUR_ADDR = 0x0A,
        RTC_ALARM1_DAY_ADDR = 0x0B,
        RTC_ALARM1_MONTH_ADDR = 0x0C,

        //RTC alarm2
        RTC_ALARM2_MINUTE_ADDR = 0x0D,
        RTC_ALARM2_HOUR_ADDR = 0x0E,
        RTC_ALARM2_WEEKDAY_ADDR = 0x0F,

        //RTC alarm enables
        RTC_ALARM_ENABLES_ADDR = 0x10,

        //RTC timestamp1 (TSR1)
        RTC_TIMESTAMP1_SECONDS = 0x11,
        RTC_TIMESTAMP1_MINUTES = 0x12,
        RTC_TIMESTAMP1_HOUR_ADDR = 0x13,
        RTC_TIMESTAMP1_DAY_ADDR = 0x14,
        RTC_TIMESTAMP1_MONTH_ADDR = 0x15,
        RTC_TIMESTAMP1_YEAR_ADDR = 0x16,

        //RTC timestamp2 (TSR2)
        RTC_TIMESTAMP2_SECONDS = 0x17,
        RTC_TIMESTAMP2_MINUTES = 0x18,
        RTC_TIMESTAMP2_HOUR_ADDR = 0x19,
        RTC_TIMESTAMP2_DAY_ADDR = 0x1A,
        RTC_TIMESTAMP2_MONTH_ADDR = 0x1B,
        RTC_TIMESTAMP2_YEAR_ADDR = 0x1C,

        //RTC timestamp3 (TSR3)
        RTC_TIMESTAMP3_SECONDS = 0x1D,
        RTC_TIMESTAMP3_MINUTES = 0x1E,
        RTC_TIMESTAMP3_HOUR_ADDR = 0x1F,
        RTC_TIMESTAMP3_DAY_ADDR = 0x20,
        RTC_TIMESTAMP3_MONTH_ADDR = 0x21,
        RTC_TIMESTAMP3_YEAR_ADDR = 0x22,

        //Stop-watch timestamp mode control
        RTC_TSR_MODE_ADDR = 0x23
    }
}