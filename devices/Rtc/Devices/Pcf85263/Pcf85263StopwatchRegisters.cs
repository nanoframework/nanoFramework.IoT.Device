// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Rtc
{
    internal enum PCF85263StopwatchRegisters : byte
    {
        //Stop-watch time registers
        SW_SECOND_100TH_ADDR = 0x00,
        SW_SECOND_ADDR = 0x01,
        SW_MINUTE_ADDR = 0x02,
        SW_HOUR_XX_XX_00_ADDR = 0x03,
        SW_HOUR_XX_00_XX_ADDR = 0x04,
        SW_HOUR_00_XX_XX_ADDR = 0x05,
        //0x05 not used
        //0x06 not used

        //Stop-watch alarm1
        SW_ALARM1_SECOND_ADDR = 0x08,
        SW_ALARM1_MINUTE_ADDR = 0x09,
        SW_ALARM1_HOUR_XX_XX_00_ADDR = 0x0A,
        SW_ALARM1_HOUR_XX_00_XX_ADDR = 0x0B,
        SW_ALARM1_HOUR_00_XX_XX_ADDR = 0x0C,

        //Stop-watch alarm2
        SW_ALARM2_MINUTE_ADDR = 0x0D,
        SW_ALARM2_HOUR_XX_00_ADDR = 0x0E,
        SW_ALARM2_HOUR_00_XX_ADDR = 0x0F,

        //Stop-watch alarm enables
        SW_ALARM_ENABLES_ADDR = 0x10,

        //Stop-watch timestamp1 (TSR1)
        SW_TIMESTAMP1_SECONDS = 0x11,
        SW_TIMESTAMP1_MINUTES = 0x12,
        SW_TIMESTAMP1_HOUR_XX_XX_00_ADDR = 0x13,
        SW_TIMESTAMP1_HOUR_XX_00_XX_ADDR = 0x14,
        SW_TIMESTAMP1_HOUR_00_XX_XX_ADDR = 0x15,
        //0x16 not used

        //Stop-watch timestamp2 (TSR2)
        SW_TIMESTAMP2_SECONDS = 0x17,
        SW_TIMESTAMP2_MINUTES = 0x18,
        SW_TIMESTAMP2_HOUR_XX_XX_00_ADDR = 0x19,
        SW_TIMESTAMP2_HOUR_XX_00_XX_ADDR = 0x1A,
        SW_TIMESTAMP2_HOUR_00_XX_XX_ADDR = 0x1B,
        //0x1C not used

        //Stop-watch timestamp3 (TSR3)
        SW_TIMESTAMP3_SECONDS = 0x1D,
        SW_TIMESTAMP3_MINUTES = 0x1E,
        SW_TIMESTAMP3_HOUR_XX_XX_00_ADDR = 0x1F,
        SW_TIMESTAMP3_HOUR_XX_00_XX_ADDR = 0x20,
        SW_TIMESTAMP3_HOUR_00_XX_XX_ADDR = 0x21,
        //0x22 not used

        //Stop-watch timestamp mode control
        SW_TSR_MODE_ADDR = 0x23
    }
}