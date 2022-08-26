// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Common;
using Iot.Device.Mcp7940xx;
using nanoFramework.TestFramework;
using System;
using System.Device.I2c;

namespace Iot.Device.NFUnitTest
{
    internal static class TestHelper
    {
        static internal string GetRegisterName(Register register)
        {
            switch (register)
            {
                case Register.TimekeepingSecond:
                    return "TimekeepingSecond";

                case Register.TimekeepingMinute:
                    return "TimekeepingMinute";

                case Register.TimekeepingHour:
                    return "TimekeepingHour";

                case Register.TimekeepingWeekday:
                    return "TimekeepingWeekday";

                case Register.TimekeepingDay:
                    return "TimekeepingDay";

                case Register.TimekeepingMonth:
                    return "TimekeepingMonth";

                case Register.TimekeepingYear:
                    return "TimekeepingYear";

                case Register.Control:
                    return "Control";

                case Register.OscillatorTrimming:
                    return "OscillatorTrimming";

                case Register.EepromUnlock:
                    return "EEPROMUnlock";

                case Register.Alarm1Second:
                    return "Alarm1Second";

                case Register.Alarm1Minute:
                    return "Alarm1Minute";

                case Register.Alarm1Hour:
                    return "Alarm1Hour";

                case Register.Alarm1Weekday:
                    return "Alarm1Weekday";

                case Register.Alarm1Day:
                    return "Alarm1Day";

                case Register.Alarm1Month:
                    return "Alarm1Month";

                case Register.Alarm2Second:
                    return "Alarm2Second";

                case Register.Alarm2Minute:
                    return "Alarm2Minute";

                case Register.Alarm2Hour:
                    return "Alarm2Hour";

                case Register.Alarm2Weekday:
                    return "Alarm2Weekday";

                case Register.Alarm2Day:
                    return "Alarm2Day";

                case Register.Alarm2Month:
                    return "Alarm2Month";

                case Register.PowerDownMinute:
                    return "PowerDownMinute";

                case Register.PowerDownHour:
                    return "PowerDownHour";

                case Register.PowerDownDay:
                    return "PowerDownDay";

                case Register.PowerDownMonth:
                    return "PowerDownMonth";

                case Register.PowerUpMinute:
                    return "PowerUpMinute";

                case Register.PowerUpHour:
                    return "PowerUpHour";

                case Register.PowerUpDay:
                    return "PowerUpDay";

                case Register.PowerUpMonth:
                    return "PowerUpMonth";

                default:
                    return "Unrecognized Register";
            }
        }

        static internal void AssertRegistersEqual(SpanByte registerBankA, SpanByte registerB, Register register)
        {
            Assert.Equal(registerBankA[(byte)register], registerB[(byte)register], message: GetRegisterName(register));
        }

        static internal void AssertRegistersNotEqual(SpanByte registerA, SpanByte registerB, Register register)
        {
            Assert.NotEqual(registerA[(byte)register], registerB[(byte)register], message: GetRegisterName(register));
        }

        static internal void AssertMaskedRegistersEqual(byte registerA, byte registerB, byte bitMask)
        {
            Assert.Equal(registerA & bitMask, registerB & bitMask);
        }

        static internal void AssertMaskedRegistersEqual(SpanByte registerA, SpanByte registerB, Register register, byte bitMask)
        {
            Assert.Equal(registerA[(byte)register] & bitMask, registerB[(byte)register] & bitMask, message: GetRegisterName(register));
        }

        static internal void AssertMaskedRegistersNotEqual(SpanByte registerA, SpanByte registerB, Register register, byte bitMask)
        {
            Assert.NotEqual(registerA[(byte)register] & bitMask, registerB[(byte)register] & bitMask, message: GetRegisterName(register));
        }

    }
}
