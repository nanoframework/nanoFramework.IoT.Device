// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Common;
using Iot.Device.Mcp7940xx;
using nanoFramework.Hardware.Esp32;
using nanoFramework.TestFramework;
using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

namespace Iot.Device.NFUnitTest
{
    [TestClass]
    public class Mcp7940mTests
    {
        static I2cDevice _i2cDevice;
        static Mcp7940m _clock;

        private Span<byte> ReadAllBuffers()
        {
            // Read all Mcp7940m registers.
            Span<byte> readBuffer = new byte[23];

            _i2cDevice.WriteByte((byte)Register.TimekeepingSecond);
            _i2cDevice.Read(readBuffer);

            return readBuffer;
        }

        [Setup]
        public void SetupMcp7940mTests()
        {
            try
            {
                Debug.WriteLine("Please adjust for your own usage. If you need another hardware, please add the proper nuget and adjust as well");

                // Setup ESP32 I2C port.
                Configuration.SetPinFunction(Gpio.IO22, DeviceFunction.I2C1_CLOCK);
                Configuration.SetPinFunction(Gpio.IO21, DeviceFunction.I2C1_DATA);

                // Setup Mcp7940m device. 
                I2cConnectionSettings i2cSettings = new I2cConnectionSettings(1, Mcp7940m.DefaultI2cAddress);
                _i2cDevice = new I2cDevice(i2cSettings);
                _clock = new Mcp7940m(_i2cDevice, ClockSource.ExternalCrystal);

                // Stop clock updating itself during testing.
                _clock.Halt();

                // Set clock to default state.
                _clock.SetTime(new DateTime(2000, 1, 1, 1, 1, 1));

                Mcp7940m.Alarm alarm1 = new Mcp7940m.Alarm(AlarmMatchMode.Full);
                _clock.SetAlarm1(alarm1);

                Mcp7940m.Alarm alarm2 = new Mcp7940m.Alarm(AlarmMatchMode.Full);
                _clock.SetAlarm2(alarm2);
            }
            catch
            {
                Assert.SkipTest("I2C port not supported in this platform or not properly configured");
            }

        }

        [TestMethod]
        public void Constructor_Cannot_Create_With_Null_I2C_Device()
        {
            Assert.Throws(typeof(ArgumentNullException), () => new Mcp7940m(null, ClockSource.ExternalCrystal));
        }

        [TestMethod]
        public void Constructor_ClockSource_Correctly_Sets_Flag()
        {
            I2cConnectionSettings i2cSettings = new I2cConnectionSettings(1, Mcp7940m.DefaultI2cAddress);
            I2cDevice i2cDevice = new I2cDevice(i2cSettings);

            Mcp7940m clock = new Mcp7940m(i2cDevice, ClockSource.ExternalClockInput);

            // Verify flag matches function returned state.
            Assert.True(RegisterHelper.RegisterBitIsSet(i2cDevice, (byte)Register.Control, (byte)RegisterMask.ExternalClockInputMask));

            clock = new Mcp7940m(i2cDevice, ClockSource.ExternalCrystal);

            // Verify flag matches function returned state.
            Assert.False(RegisterHelper.RegisterBitIsSet(i2cDevice, (byte)Register.Control, (byte)RegisterMask.ExternalClockInputMask));
        }

        [TestMethod]
        public void Constructor_Clock_Is_In_24_Hour_Mode()
        {
            Assert.False(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.TimekeepingHour, (byte)RegisterMask.ClockTimeFormatMask));
        }

        [TestMethod]
        public void SetTime_Only_Changes_Relevant_Registers()
        {
            Span<byte> before = ReadAllBuffers();

            _clock.SetTime(new DateTime(2099, 12, 31, 23, 59, 15));

            Span<byte> after = ReadAllBuffers();

            // Verify time registers have changed.
            TestHelper.AssertRegistersNotEqual(before, after, Register.TimekeepingSecond);
            TestHelper.AssertRegistersNotEqual(before, after, Register.TimekeepingMinute);
            TestHelper.AssertRegistersNotEqual(before, after, Register.TimekeepingHour);
            TestHelper.AssertRegistersNotEqual(before, after, Register.TimekeepingWeekday);
            TestHelper.AssertRegistersNotEqual(before, after, Register.TimekeepingDay);
            TestHelper.AssertRegistersNotEqual(before, after, Register.TimekeepingMonth);
            TestHelper.AssertRegistersNotEqual(before, after, Register.TimekeepingYear);

            // Verify oscillator input flag has not been altered.
            TestHelper.AssertMaskedRegistersEqual(before, after, Register.TimekeepingSecond, (byte)RegisterMask.OscillatorInputEnabledMask);

            // Verify oscillator running flag has not been altered.
            TestHelper.AssertMaskedRegistersEqual(before, after, Register.TimekeepingWeekday, (byte)RegisterMask.OscillatorRunningMask);

            // Verify control registers.
            TestHelper.AssertRegistersEqual(before, after, Register.Control);
            TestHelper.AssertRegistersEqual(before, after, Register.OscillatorTrimming);
            TestHelper.AssertRegistersEqual(before, after, Register.EepromUnlock);

            // Verify Alarm1 registers.
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Second);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Minute);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Hour);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Weekday);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Day);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Month);

            // Verify Alarm2 registers.
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Second);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Minute);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Hour);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Weekday);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Day);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Month);
        }

        #region Alarm1

        [TestMethod]
        public void Alarm1_SetAlarm1_Correctly_Sets_Registers()
        {
            Mcp7940m.Alarm alarm;

            // ---------------------
            // Second
            // ---------------------

            // Verify match mode is correctly set for second.
            alarm = new Mcp7940m.Alarm(AlarmMatchMode.Second, second: 0);
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)AlarmMatchMode.Second, (byte)RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Weekday) & (byte)RegisterMask.AlarmMatchModeMask);

            // Verify second register is correctly set.
            alarm.Second = 0;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x00, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Second));

            alarm.Second = 15;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x15, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Second));

            alarm.Second = 30;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x30, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Second));

            alarm.Second = 45;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x45, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Second));

            alarm.Second = 59;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x59, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Second));

            // ---------------------
            // Minute
            // ---------------------

            // Verify match mode is correctly set for minutes.
            alarm = new Mcp7940m.Alarm(AlarmMatchMode.Minute, minute: 0);
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)AlarmMatchMode.Minute, (byte)RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Weekday) & (byte)RegisterMask.AlarmMatchModeMask);

            // Verify minute register is correctly set.
            alarm.Minute = 0;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x00, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Minute));

            alarm.Minute = 15;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x15, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Minute));

            alarm.Minute = 30;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x30, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Minute));

            alarm.Minute = 45;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x45, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Minute));

            alarm.Minute = 59;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x59, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Minute));

            // ---------------------
            // Hour
            // ---------------------

            // Verify match mode is correctly set for hours.
            alarm = new Mcp7940m.Alarm(AlarmMatchMode.Hour, hour: 0);
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)AlarmMatchMode.Hour, (byte)RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Weekday) & (byte)RegisterMask.AlarmMatchModeMask);

            // Verify hour register is correctly set.
            alarm.Hour = 0;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x00, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Hour));

            alarm.Hour = 6;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x06, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Hour));

            alarm.Hour = 12;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x12, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Hour));

            alarm.Hour = 18;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x18, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Hour));

            alarm.Hour = 23;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x23, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Hour));

            // ---------------------
            // Day
            // ---------------------

            // Verify match mode is correctly set for days.
            alarm = new Mcp7940m.Alarm(AlarmMatchMode.Day, day: 0);
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)AlarmMatchMode.Day, (byte)RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Weekday) & (byte)RegisterMask.AlarmMatchModeMask);

            // Verify day register is correctly set.
            alarm.Day = 1;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x01, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Day));

            alarm.Day = 7;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x07, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Day));

            alarm.Day = 15;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x15, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Day));

            alarm.Day = 22;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x22, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Day));

            alarm.Day = 31;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x31, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Day));

            // ---------------------
            // Day-Of-Week
            // ---------------------

            // Verify match mode is correctly set for day-of-week.
            alarm = new Mcp7940m.Alarm(AlarmMatchMode.DayOfWeek, day: 0);
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)AlarmMatchMode.DayOfWeek, (byte)RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Weekday) & (byte)RegisterMask.AlarmMatchModeMask);

            byte before = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Weekday);

            // Verify weekday register is correctly set.
            alarm.DayOfWeek = DayOfWeek.Sunday;
            _clock.SetAlarm1(alarm);
            byte after = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Weekday);
            TestHelper.AssertMaskedRegistersEqual(after, (byte)DayOfWeek.Sunday, (byte)RegisterMask.AlarmDayOfWeekMask);
            TestHelper.AssertMaskedRegistersEqual(before, after, (byte)~RegisterMask.AlarmDayOfWeekMask);

            alarm.DayOfWeek = DayOfWeek.Monday;
            _clock.SetAlarm1(alarm);
            after = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Weekday);
            TestHelper.AssertMaskedRegistersEqual(after, (byte)DayOfWeek.Monday, (byte)RegisterMask.AlarmDayOfWeekMask);
            TestHelper.AssertMaskedRegistersEqual(before, after, (byte)~RegisterMask.AlarmDayOfWeekMask);

            alarm.DayOfWeek = DayOfWeek.Tuesday;
            _clock.SetAlarm1(alarm);
            after = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Weekday);
            TestHelper.AssertMaskedRegistersEqual(after, (byte)DayOfWeek.Tuesday, (byte)RegisterMask.AlarmDayOfWeekMask);
            TestHelper.AssertMaskedRegistersEqual(before, after, (byte)~RegisterMask.AlarmDayOfWeekMask);

            alarm.DayOfWeek = DayOfWeek.Wednesday;
            _clock.SetAlarm1(alarm);
            after = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Weekday);
            TestHelper.AssertMaskedRegistersEqual(after, (byte)DayOfWeek.Wednesday, (byte)RegisterMask.AlarmDayOfWeekMask);
            TestHelper.AssertMaskedRegistersEqual(before, after, (byte)~RegisterMask.AlarmDayOfWeekMask);

            alarm.DayOfWeek = DayOfWeek.Thursday;
            _clock.SetAlarm1(alarm);
            after = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Weekday);
            TestHelper.AssertMaskedRegistersEqual(after, (byte)DayOfWeek.Thursday, (byte)RegisterMask.AlarmDayOfWeekMask);
            TestHelper.AssertMaskedRegistersEqual(before, after, (byte)~RegisterMask.AlarmDayOfWeekMask);

            alarm.DayOfWeek = DayOfWeek.Friday;
            _clock.SetAlarm1(alarm);
            after = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Weekday);
            TestHelper.AssertMaskedRegistersEqual(after, (byte)DayOfWeek.Friday, (byte)RegisterMask.AlarmDayOfWeekMask);
            TestHelper.AssertMaskedRegistersEqual(before, after, (byte)~RegisterMask.AlarmDayOfWeekMask);

            alarm.DayOfWeek = DayOfWeek.Saturday;
            _clock.SetAlarm1(alarm);
            after = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Weekday);
            TestHelper.AssertMaskedRegistersEqual(after, (byte)DayOfWeek.Saturday, (byte)RegisterMask.AlarmDayOfWeekMask);
            TestHelper.AssertMaskedRegistersEqual(before, after, (byte)~RegisterMask.AlarmDayOfWeekMask);

            // ---------------------
            // Month
            // ---------------------

            // Verify match mode is correctly set for full match.
            alarm = new Mcp7940m.Alarm(AlarmMatchMode.Full, day: 0);
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)AlarmMatchMode.Full, (byte)RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Weekday) & (byte)RegisterMask.AlarmMatchModeMask);

            // Verify month register is correctly set.
            alarm.Month = 1;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x01, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Month));

            alarm.Month = 2;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x02, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Month));

            alarm.Month = 3;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x03, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Month));

            alarm.Month = 4;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x04, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Month));

            alarm.Month = 5;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x05, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Month));

            alarm.Month = 6;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x06, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Month));

            alarm.Month = 7;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x07, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Month));

            alarm.Month = 8;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x08, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Month));

            alarm.Month = 9;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x09, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Month));

            alarm.Month = 10;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x10, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Month));

            alarm.Month = 11;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x11, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Month));

            alarm.Month = 12;
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)0x12, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Month));
        }

        [TestMethod]
        public void Alarm1_SetAlarm1_Only_Changes_Relevant_Registers()
        {
            // Set alarm to known state.
            Mcp7940m.Alarm alarm = new Mcp7940m.Alarm(AlarmMatchMode.Full);
            _clock.SetAlarm1(alarm);

            Span<byte> before = ReadAllBuffers();

            alarm = new Mcp7940m.Alarm(AlarmMatchMode.Full, 12, 31, 23, 33, 33, DayOfWeek.Thursday);
            _clock.SetAlarm1(alarm);

            Span<byte> after = ReadAllBuffers();

            // Verify time keeping registers.
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingSecond);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingMinute);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingHour);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingWeekday);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingDay);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingMonth);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingYear);

            // Verify control registers.
            TestHelper.AssertRegistersEqual(before, after, Register.Control);
            TestHelper.AssertRegistersEqual(before, after, Register.OscillatorTrimming);
            TestHelper.AssertRegistersEqual(before, after, Register.EepromUnlock);

            // Verify Alarm1 registers have been altered.
            TestHelper.AssertRegistersNotEqual(before, after, Register.Alarm1Second);
            TestHelper.AssertRegistersNotEqual(before, after, Register.Alarm1Minute);
            TestHelper.AssertRegistersNotEqual(before, after, Register.Alarm1Hour);
            TestHelper.AssertRegistersNotEqual(before, after, Register.Alarm1Weekday);
            TestHelper.AssertRegistersNotEqual(before, after, Register.Alarm1Day);
            TestHelper.AssertRegistersNotEqual(before, after, Register.Alarm1Month);

            // Verify Alarm2 registers.
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Second);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Minute);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Hour);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Weekday);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Day);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Month);
        }

        [TestMethod]
        public void Alarm1_EnableAlarm1_And_DisableAlarm1_Only_Changes_Relevant_Flag()
        {
            _clock.EnableAlarm1();
            Span<byte> before = ReadAllBuffers();

            _clock.DisableAlarm1();
            Span<byte> after = ReadAllBuffers();

            // Verify time keeping registers.
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingSecond);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingMinute);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingHour);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingWeekday);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingDay);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingMonth);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingYear);

            // Verify control registers.
            TestHelper.AssertRegistersEqual(before, after, Register.OscillatorTrimming);
            TestHelper.AssertRegistersEqual(before, after, Register.EepromUnlock);

            // Verify only Alarm1 Enable flag has been altered.
            TestHelper.AssertMaskedRegistersEqual(before, after, Register.Control, (byte)~RegisterMask.Alarm1InterruptEnabledMask);
            TestHelper.AssertMaskedRegistersNotEqual(before, after, Register.Control, (byte)RegisterMask.Alarm1InterruptEnabledMask);

            // Verify Alarm1 registers.
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Second);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Minute);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Hour);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Day);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Month);

            // Verify Alarm2 registers.
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Second);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Minute);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Hour);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Day);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Month);
        }

        [TestMethod]
        public void Alarm1_EnableAlarm1_And_DisableAlarm1_Correctly_Sets_Flag()
        {
            _clock.EnableAlarm1();

            // Verify flag has been set for Alarm1.
            Assert.True(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.Alarm1InterruptEnabledMask));

            _clock.DisableAlarm1();

            // Verify flag has been cleared for Alarm1.
            Assert.False(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.Alarm1InterruptEnabledMask));
        }

        [TestMethod]
        public void Alarm1_IsEnabledAlarm1_Property_Correctly_Gets_Flag()
        {
            _clock.EnableAlarm1();

            // Verify flag matches function returned state.
            bool isEnabled = _clock.IsEnabledAlarm1;
            Assert.Equal(isEnabled, RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.Alarm1InterruptEnabledMask));

            _clock.DisableAlarm1();

            // Verify flag matches function returned state.
            isEnabled = _clock.IsEnabledAlarm1;
            Assert.Equal(isEnabled, RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.Alarm1InterruptEnabledMask));
        }

        [TestMethod]
        public void Alarm1_IsTriggeredAlarm1_Property_Test()
        {
            Mcp7940m.Alarm alarm = new Mcp7940m.Alarm(AlarmMatchMode.Minute, minute: 34);
            _clock.SetAlarm1(alarm);
            _clock.EnableAlarm1();

            // Clear Alarm1.
            _clock.ResetAlarm1();
            Assert.False(_clock.IsTriggeredAlarm1);

            // Set time to trigger alarm.
            _clock.SetTime(new DateTime(2022, 8, 15, 23, 33, 59));

            // Start clock.
            _clock.StartClock(true);

            // Wait for one second for clock to update its second register.
            Thread.Sleep(1000);

            Assert.True(_clock.IsTriggeredAlarm1);

            _clock.Halt();
        }

        #endregion

        #region Alarm2

        [TestMethod]
        public void Alarm2_SetAlarm2_Correctly_Sets_Registers()
        {
            Mcp7940m.Alarm alarm;

            // ---------------------
            // Second
            // ---------------------

            // Verify match mode is correctly set for second.
            alarm = new Mcp7940m.Alarm(AlarmMatchMode.Second, second: 0);
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)AlarmMatchMode.Second, (byte)RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Weekday) & (byte)RegisterMask.AlarmMatchModeMask);

            // Verify second register is correctly set.
            alarm.Second = 0;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x00, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Second));

            alarm.Second = 15;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x15, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Second));

            alarm.Second = 30;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x30, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Second));

            alarm.Second = 45;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x45, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Second));

            alarm.Second = 59;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x59, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Second));

            // ---------------------
            // Minute
            // ---------------------

            // Verify match mode is correctly set for minutes.
            alarm = new Mcp7940m.Alarm(AlarmMatchMode.Minute, minute: 0);
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)AlarmMatchMode.Minute, (byte)RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Weekday) & (byte)RegisterMask.AlarmMatchModeMask);

            // Verify minute register is correctly set.
            alarm.Minute = 0;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x00, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Minute));

            alarm.Minute = 15;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x15, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Minute));

            alarm.Minute = 30;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x30, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Minute));

            alarm.Minute = 45;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x45, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Minute));

            alarm.Minute = 59;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x59, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Minute));

            // ---------------------
            // Hour
            // ---------------------

            // Verify match mode is correctly set for hours.
            alarm = new Mcp7940m.Alarm(AlarmMatchMode.Hour, hour: 0);
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)AlarmMatchMode.Hour, (byte)RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Weekday) & (byte)RegisterMask.AlarmMatchModeMask);

            // Verify hour register is correctly set.
            alarm.Hour = 0;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x00, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Hour));

            alarm.Hour = 6;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x06, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Hour));

            alarm.Hour = 12;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x12, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Hour));

            alarm.Hour = 18;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x18, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Hour));

            alarm.Hour = 23;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x23, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Hour));

            // ---------------------
            // Day
            // ---------------------

            // Verify match mode is correctly set for days.
            alarm = new Mcp7940m.Alarm(AlarmMatchMode.Day, day: 0);
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)AlarmMatchMode.Day, (byte)RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Weekday) & (byte)RegisterMask.AlarmMatchModeMask);

            // Verify day register is correctly set.
            alarm.Day = 1;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x01, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Day));

            alarm.Day = 7;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x07, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Day));

            alarm.Day = 15;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x15, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Day));

            alarm.Day = 22;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x22, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Day));

            alarm.Day = 31;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x31, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Day));

            // ---------------------
            // Day-Of-Week
            // ---------------------

            // Verify match mode is correctly set for day-of-week.
            alarm = new Mcp7940m.Alarm(AlarmMatchMode.DayOfWeek, day: 0);
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)AlarmMatchMode.DayOfWeek, (byte)RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Weekday) & (byte)RegisterMask.AlarmMatchModeMask);

            byte before = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Weekday);

            // Verify weekday register is correctly set.
            alarm.DayOfWeek = DayOfWeek.Sunday;
            _clock.SetAlarm2(alarm);
            byte after = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Weekday);
            TestHelper.AssertMaskedRegistersEqual(after, (byte)DayOfWeek.Sunday, (byte)RegisterMask.AlarmDayOfWeekMask);
            TestHelper.AssertMaskedRegistersEqual(before, after, (byte)~RegisterMask.AlarmDayOfWeekMask);

            alarm.DayOfWeek = DayOfWeek.Monday;
            _clock.SetAlarm2(alarm);
            after = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Weekday);
            TestHelper.AssertMaskedRegistersEqual(after, (byte)DayOfWeek.Monday, (byte)RegisterMask.AlarmDayOfWeekMask);
            TestHelper.AssertMaskedRegistersEqual(before, after, (byte)~RegisterMask.AlarmDayOfWeekMask);

            alarm.DayOfWeek = DayOfWeek.Tuesday;
            _clock.SetAlarm2(alarm);
            after = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Weekday);
            TestHelper.AssertMaskedRegistersEqual(after, (byte)DayOfWeek.Tuesday, (byte)RegisterMask.AlarmDayOfWeekMask);
            TestHelper.AssertMaskedRegistersEqual(before, after, (byte)~RegisterMask.AlarmDayOfWeekMask);

            alarm.DayOfWeek = DayOfWeek.Wednesday;
            _clock.SetAlarm2(alarm);
            after = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Weekday);
            TestHelper.AssertMaskedRegistersEqual(after, (byte)DayOfWeek.Wednesday, (byte)RegisterMask.AlarmDayOfWeekMask);
            TestHelper.AssertMaskedRegistersEqual(before, after, (byte)~RegisterMask.AlarmDayOfWeekMask);

            alarm.DayOfWeek = DayOfWeek.Thursday;
            _clock.SetAlarm2(alarm);
            after = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Weekday);
            TestHelper.AssertMaskedRegistersEqual(after, (byte)DayOfWeek.Thursday, (byte)RegisterMask.AlarmDayOfWeekMask);
            TestHelper.AssertMaskedRegistersEqual(before, after, (byte)~RegisterMask.AlarmDayOfWeekMask);

            alarm.DayOfWeek = DayOfWeek.Friday;
            _clock.SetAlarm2(alarm);
            after = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Weekday);
            TestHelper.AssertMaskedRegistersEqual(after, (byte)DayOfWeek.Friday, (byte)RegisterMask.AlarmDayOfWeekMask);
            TestHelper.AssertMaskedRegistersEqual(before, after, (byte)~RegisterMask.AlarmDayOfWeekMask);

            alarm.DayOfWeek = DayOfWeek.Saturday;
            _clock.SetAlarm2(alarm);
            after = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Weekday);
            TestHelper.AssertMaskedRegistersEqual(after, (byte)DayOfWeek.Saturday, (byte)RegisterMask.AlarmDayOfWeekMask);
            TestHelper.AssertMaskedRegistersEqual(before, after, (byte)~RegisterMask.AlarmDayOfWeekMask);

            // ---------------------
            // Month
            // ---------------------

            // Verify match mode is correctly set for full match.
            alarm = new Mcp7940m.Alarm(AlarmMatchMode.Full, day: 0);
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)AlarmMatchMode.Full, (byte)RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Weekday) & (byte)RegisterMask.AlarmMatchModeMask);

            // Verify month register is correctly set.
            alarm.Month = 1;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x01, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Month));

            alarm.Month = 2;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x02, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Month));

            alarm.Month = 3;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x03, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Month));

            alarm.Month = 4;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x04, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Month));

            alarm.Month = 5;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x05, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Month));

            alarm.Month = 6;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x06, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Month));

            alarm.Month = 7;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x07, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Month));

            alarm.Month = 8;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x08, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Month));

            alarm.Month = 9;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x09, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Month));

            alarm.Month = 10;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x10, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Month));

            alarm.Month = 11;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x11, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Month));

            alarm.Month = 12;
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)0x12, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Month));
        }

        [TestMethod]
        public void Alarm2_SetAlarm2_Only_Changes_Relevant_Registers()
        {
            // Set alarm to known state.
            Mcp7940m.Alarm alarm = new Mcp7940m.Alarm(AlarmMatchMode.Full);
            _clock.SetAlarm2(alarm);

            Span<byte> before = ReadAllBuffers();

            alarm = new Mcp7940m.Alarm(AlarmMatchMode.Full, 12, 31, 23, 33, 33, DayOfWeek.Thursday);
            _clock.SetAlarm2(alarm);

            Span<byte> after = ReadAllBuffers();

            // Verify time keeping registers.
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingSecond);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingMinute);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingHour);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingWeekday);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingDay);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingMonth);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingYear);

            // Verify control registers.
            TestHelper.AssertRegistersEqual(before, after, Register.Control);
            TestHelper.AssertRegistersEqual(before, after, Register.OscillatorTrimming);
            TestHelper.AssertRegistersEqual(before, after, Register.EepromUnlock);

            // Verify Alarm1 registers have been altered.
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Second);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Minute);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Hour);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Weekday);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Day);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Month);

            // Verify Alarm2 registers.
            TestHelper.AssertRegistersNotEqual(before, after, Register.Alarm2Second);
            TestHelper.AssertRegistersNotEqual(before, after, Register.Alarm2Minute);
            TestHelper.AssertRegistersNotEqual(before, after, Register.Alarm2Hour);
            TestHelper.AssertRegistersNotEqual(before, after, Register.Alarm2Weekday);
            TestHelper.AssertRegistersNotEqual(before, after, Register.Alarm2Day);
            TestHelper.AssertRegistersNotEqual(before, after, Register.Alarm2Month);
        }

        [TestMethod]
        public void Alarm2_EnableAlarm2_And_DisableAlarm2_Only_Changes_Relevant_Flag()
        {
            _clock.EnableAlarm2();
            Span<byte> before = ReadAllBuffers();

            _clock.DisableAlarm2();
            Span<byte> after = ReadAllBuffers();

            // Verify time keeping registers.
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingSecond);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingMinute);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingHour);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingWeekday);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingDay);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingMonth);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingYear);

            // Verify control registers.
            TestHelper.AssertRegistersEqual(before, after, Register.OscillatorTrimming);
            TestHelper.AssertRegistersEqual(before, after, Register.EepromUnlock);

            // Verify only Alarm2 Enable flag has been altered.
            TestHelper.AssertMaskedRegistersEqual(before, after, Register.Control, (byte)~RegisterMask.Alarm2InterruptEnabledMask);
            TestHelper.AssertMaskedRegistersNotEqual(before, after, Register.Control, (byte)RegisterMask.Alarm2InterruptEnabledMask);

            // Verify Alarm2 registers.
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Second);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Minute);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Hour);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Day);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Month);

            // Verify Alarm2 registers.
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Second);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Minute);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Hour);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Day);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Month);
        }

        [TestMethod]
        public void Alarm2_EnableAlarm2_And_DisableAlarm2_Correctly_Sets_Flag()
        {
            _clock.EnableAlarm2();

            // Verify flag has been set for Alarm2.
            Assert.True(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.Alarm2InterruptEnabledMask));

            _clock.DisableAlarm2();

            // Verify flag has been cleared for Alarm2.
            Assert.False(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.Alarm2InterruptEnabledMask));
        }

        [TestMethod]
        public void Alarm2_IsEnabledAlarm2_Property_Correctly_Gets_Flag()
        {
            _clock.EnableAlarm2();

            // Verify flag matches function returned state.
            bool isEnabled = _clock.IsEnabledAlarm2;
            Assert.Equal(isEnabled, RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.Alarm2InterruptEnabledMask));

            _clock.DisableAlarm2();

            // Verify flag matches function returned state for both Alarm2 and Alarm2.
            isEnabled = _clock.IsEnabledAlarm2;
            Assert.Equal(isEnabled, RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.Alarm2InterruptEnabledMask));
        }

        [TestMethod]
        public void Alarm2_IsTriggeredAlarm2_Property_Test()
        {
            Mcp7940m.Alarm alarm = new Mcp7940m.Alarm(AlarmMatchMode.Minute, minute: 34);
            _clock.SetAlarm2(alarm);
            _clock.EnableAlarm2();

            // Clear Alarm2.
            _clock.ResetAlarm2();
            Assert.False(_clock.IsTriggeredAlarm2);

            // Set time to trigger alarm.
            _clock.SetTime(new DateTime(2022, 8, 15, 23, 33, 59));

            // Start clock.
            _clock.StartClock(true);

            // Wait for one second for clock to update its second register.
            Thread.Sleep(1000);

            Assert.True(_clock.IsTriggeredAlarm2);

            _clock.Halt();
        }

        #endregion

        #region AlarmInterruptPolarity

        [TestMethod]
        public void AlarmInterruptPolarity_Property_Only_Changes_Relevant_Flag()
        {
            _clock.AlarmInterruptPolarity = PinValue.High;
            Span<byte> before = ReadAllBuffers();

            _clock.AlarmInterruptPolarity = PinValue.Low;
            Span<byte> after = ReadAllBuffers();

            // Verify time keeping registers.
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingSecond);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingMinute);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingHour);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingWeekday);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingDay);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingMonth);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingYear);

            // Verify control registers.
            TestHelper.AssertRegistersEqual(before, after, Register.Control);
            TestHelper.AssertRegistersEqual(before, after, Register.OscillatorTrimming);
            TestHelper.AssertRegistersEqual(before, after, Register.EepromUnlock);

            // Verify Alarm1 registers.
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Second);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Minute);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Hour);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Day);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Month);

            // Verify only AlarmInterruptPolarity flag has been altered on Alarm1.
            TestHelper.AssertMaskedRegistersEqual(before, after, Register.Alarm1Weekday, (byte)~RegisterMask.AlarmInterruptPolarityMask);
            TestHelper.AssertMaskedRegistersNotEqual(before, after, Register.Alarm1Weekday, (byte)RegisterMask.AlarmInterruptPolarityMask);

            // Verify Alarm2 registers.
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Second);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Minute);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Hour);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Day);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Month);

            // Verify only non-AlarmInterruptPolarity flags have not been altered on Alarm2.
            TestHelper.AssertMaskedRegistersEqual(before, after, Register.Alarm2Weekday, (byte)(RegisterMask.AlarmMatchModeMask | RegisterMask.AlarmInterruptMask | RegisterMask.AlarmDayOfWeekMask));
            TestHelper.AssertMaskedRegistersNotEqual(before, after, Register.Alarm2Weekday, (byte)RegisterMask.AlarmInterruptPolarityMask);
        }

        [TestMethod]
        public void AlarmInterruptPolarity_Property_Correctly_Gets_And_Sets_Flag()
        {
            _clock.AlarmInterruptPolarity = PinValue.High;

            // Verify flag matches function return.
            PinValue pinValue = _clock.AlarmInterruptPolarity;
            Assert.True(pinValue == PinValue.High);
            Assert.True(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Alarm1Weekday, (byte)RegisterMask.AlarmInterruptPolarityMask));

            _clock.AlarmInterruptPolarity = PinValue.Low;

            // Verify flag matches function return.
            pinValue = _clock.AlarmInterruptPolarity;
            Assert.True(pinValue == PinValue.Low);
            Assert.False(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Alarm1Weekday, (byte)RegisterMask.AlarmInterruptPolarityMask));
        }

        #endregion

        #region  General Purpose Output

        [TestMethod]
        public void GeneralPurposeOutput_Property_Only_Changes_Relevant_Flag()
        {
            _clock.GeneralPurposeOutput = PinValue.High;
            Span<byte> before = ReadAllBuffers();

            _clock.GeneralPurposeOutput = PinValue.Low;
            Span<byte> after = ReadAllBuffers();

            // Verify time keeping registers.
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingSecond);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingMinute);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingHour);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingWeekday);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingDay);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingMonth);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingYear);

            // Verify control registers.
            TestHelper.AssertRegistersEqual(before, after, Register.OscillatorTrimming);
            TestHelper.AssertRegistersEqual(before, after, Register.EepromUnlock);

            // Verify only GeneralPurposeOutput Enable flag has been altered.
            TestHelper.AssertMaskedRegistersEqual(before, after, Register.Control, (byte)~RegisterMask.GeneralPurposeOutputMask);
            TestHelper.AssertMaskedRegistersNotEqual(before, after, Register.Control, (byte)RegisterMask.GeneralPurposeOutputMask);

            // Verify Alarm1 registers.
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Second);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Minute);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Hour);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Day);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Month);

            // Verify Alarm2 registers.
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Second);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Minute);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Hour);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Day);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Month);
        }

        [TestMethod]
        public void GeneralPurposeOutput_Property_Correctly_Gets_And_Sets_Flag()
        {
            _clock.GeneralPurposeOutput = PinValue.High;

            // Verify flag matches function returned state.
            PinValue pinValue = _clock.GeneralPurposeOutput;
            Assert.True(pinValue == PinValue.High);
            Assert.True(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.GeneralPurposeOutputMask));

            _clock.GeneralPurposeOutput = PinValue.Low;

            // Verify flag matches function returned state.
            pinValue = _clock.GeneralPurposeOutput;
            Assert.True(pinValue == PinValue.Low);
            Assert.False(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.GeneralPurposeOutputMask));
        }

        #endregion

        #region Square Wave Output

        [TestMethod]
        public void SquareWaveOutput_EnableSquareWaveOutput_And_DisableSquareWaveOutput_Only_Changes_Relevant_Flag()
        {
            _clock.EnableSquareWaveOutput();
            Span<byte> before = ReadAllBuffers();

            _clock.DisableSquareWaveOutput();
            Span<byte> after = ReadAllBuffers();

            // Verify time keeping registers.
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingSecond);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingMinute);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingHour);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingWeekday);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingDay);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingMonth);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingYear);

            // Verify control registers.
            TestHelper.AssertRegistersEqual(before, after, Register.OscillatorTrimming);
            TestHelper.AssertRegistersEqual(before, after, Register.EepromUnlock);

            // Verify only SquareWaveOutput Enable flag has been altered.
            TestHelper.AssertMaskedRegistersEqual(before, after, Register.Control, (byte)~RegisterMask.SquareWaveOutputMask);
            TestHelper.AssertMaskedRegistersNotEqual(before, after, Register.Control, (byte)RegisterMask.SquareWaveOutputMask);

            // Verify Alarm1 registers.
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Second);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Minute);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Hour);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Day);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Month);

            // Verify Alarm2 registers.
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Second);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Minute);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Hour);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Day);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Month);
        }

        [TestMethod]
        public void SquareWaveOutput_EnableSquareWaveOutput_And_DisableSquareWaveOutput_Correctly_Sets_Flag()
        {
            _clock.EnableSquareWaveOutput();

            // Verify flag has been set for SquareWaveOutput.
            Assert.True(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.SquareWaveOutputMask));

            _clock.DisableSquareWaveOutput();

            // Verify flag has been cleared for SquareWaveOutput.
            Assert.False(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.SquareWaveOutputMask));
        }

        [TestMethod]
        public void SquareWaveOutput_IsEnabledSquareWaveOutput_Property_Correctly_Gets_Flag()
        {
            _clock.EnableSquareWaveOutput();

            // Verify flag matches function returned state.
            bool isEnabled = _clock.IsEnabledSquareWaveOutput;
            Assert.Equal(isEnabled, RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.SquareWaveOutputMask));

            _clock.DisableSquareWaveOutput();

            // Verify flag matches function returned state.
            isEnabled = _clock.IsEnabledSquareWaveOutput;
            Assert.Equal(isEnabled, RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.SquareWaveOutputMask));
        }

        public void SquareWaveOutput_SquareWaveOutputFrequency_Property_Only_Changes_Relevant_Flag()
        {
            _clock.SquareWaveOutputFrequency = SquareWaveFrequency.Frequency1Hz;
            Span<byte> before = ReadAllBuffers();

            _clock.SquareWaveOutputFrequency = SquareWaveFrequency.Frequency32kHz;
            Span<byte> after = ReadAllBuffers();

            // Verify time keeping registers.
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingSecond);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingMinute);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingHour);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingWeekday);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingDay);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingMonth);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingYear);

            // Verify control registers.
            TestHelper.AssertRegistersEqual(before, after, Register.OscillatorTrimming);
            TestHelper.AssertRegistersEqual(before, after, Register.EepromUnlock);

            // Verify only SquareWaveOutput Enable flag has been altered.
            TestHelper.AssertMaskedRegistersEqual(before, after, Register.Control, (byte)~RegisterMask.SquareWaveFrequencyMask);
            TestHelper.AssertMaskedRegistersNotEqual(before, after, Register.Control, (byte)RegisterMask.SquareWaveFrequencyMask);

            // Verify Alarm1 registers.
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Second);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Minute);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Hour);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Day);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Month);

            // Verify Alarm2 registers.
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Second);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Minute);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Hour);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Day);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Month);
        }

        [TestMethod]
        public void SquareWaveOutput_SquareWaveOutputFrequency_Property_Correctly_Gets_And_Sets_Flag()
        {
            _clock.SquareWaveOutputFrequency = SquareWaveFrequency.Frequency1Hz;
            Assert.Equals(SquareWaveFrequency.Frequency1Hz, _clock.SquareWaveOutputFrequency);
            TestHelper.AssertMaskedRegistersEqual((byte)SquareWaveFrequency.Frequency1Hz, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Control), (byte)RegisterMask.SquareWaveFrequencyMask);

            _clock.SquareWaveOutputFrequency = SquareWaveFrequency.Frequency4kHz;
            Assert.Equals(SquareWaveFrequency.Frequency4kHz, _clock.SquareWaveOutputFrequency);
            TestHelper.AssertMaskedRegistersEqual((byte)SquareWaveFrequency.Frequency4kHz, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Control), (byte)RegisterMask.SquareWaveFrequencyMask);

            _clock.SquareWaveOutputFrequency = SquareWaveFrequency.Frequency8kHz;
            Assert.Equals(SquareWaveFrequency.Frequency8kHz, _clock.SquareWaveOutputFrequency);
            TestHelper.AssertMaskedRegistersEqual((byte)SquareWaveFrequency.Frequency8kHz, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Control), (byte)RegisterMask.SquareWaveFrequencyMask);

            _clock.SquareWaveOutputFrequency = SquareWaveFrequency.Frequency32kHz;
            Assert.Equals(SquareWaveFrequency.Frequency32kHz, _clock.SquareWaveOutputFrequency);
            TestHelper.AssertMaskedRegistersEqual((byte)SquareWaveFrequency.Frequency32kHz, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Control), (byte)RegisterMask.SquareWaveFrequencyMask);
        }

        #endregion

        #region Clock

        public void Clock_StartClock_And_Halt_Correctly_Sets_Flag()
        {
            _clock.StartClock();
            Assert.True(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.TimekeepingSecond, (byte)RegisterMask.OscillatorInputEnabledMask));

            _clock.Halt();
            Assert.False(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.TimekeepingSecond, (byte)RegisterMask.OscillatorInputEnabledMask));
        }

        [TestMethod]
        public void Clock_StartClock_And_Halt_Only_Changes_Relevant_Flag()
        {
            _clock.StartClock();
            Span<byte> before = ReadAllBuffers();

            _clock.Halt();
            Span<byte> after = ReadAllBuffers();

            // Verify time keeping registers.
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingMinute);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingHour);
            TestHelper.AssertMaskedRegistersEqual(before, after, Register.TimekeepingWeekday, unchecked((byte)~RegisterMask.OscillatorRunningMask));
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingDay);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingMonth);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingYear);

            // Verify only Alarm1 Enable flag has been altered.
            TestHelper.AssertMaskedRegistersEqual(before, after, Register.TimekeepingSecond, unchecked((byte)~RegisterMask.OscillatorInputEnabledMask));
            TestHelper.AssertMaskedRegistersNotEqual(before, after, Register.TimekeepingSecond, (byte)RegisterMask.OscillatorInputEnabledMask);

            // Verify control registers.
            TestHelper.AssertRegistersEqual(before, after, Register.OscillatorTrimming);
            TestHelper.AssertRegistersEqual(before, after, Register.EepromUnlock);
            TestHelper.AssertRegistersEqual(before, after, Register.Control);

            // Verify Alarm1 registers.
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Second);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Minute);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Hour);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Day);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Month);

            // Verify Alarm2 registers.
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Second);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Minute);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Hour);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Day);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Month);
        }

        [TestMethod]
        public void Clock_IsHalted_Property_Correctly_Gets_Flag()
        {
            _clock.StartClock();

            // Verify flag matches function returned state.
            Assert.False(_clock.IsHalted);

            _clock.Halt();

            // Verify flag matches function returned state.
            Assert.True(_clock.IsHalted);
        }

        #endregion

        #region SRAM

        [TestMethod]
        public void SRAM_ConvertAddressToSRAM_Works_Correctly()
        {
            for (byte address = 0; address < 63; address++)
            {
                Assert.Equal(0x20 + address, _clock.ConvertAddressToSRAM(address));
            }
        }

        [TestMethod]
        public void SRAM_ConvertAddressToSRAM_Throws_Exception_When_Address_Out_Of_Range()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => _clock.ConvertAddressToSRAM(64));
        }

        [TestMethod]
        public void SRAM_ReadByteFromSRAM_And_WriteByteToSRAM_Work_Correctly()
        {
            Random random = new Random();
            byte value;

            for (byte address = 0; address < 63; address++)
            {
                value = (byte)random.Next(byte.MaxValue);

                _clock.WriteByteToSRAM(address, value);

                Assert.Equal(value, _clock.ReadByteFromSRAM(address), message: $"0x{address:X2} : Byte read does not match the byte written.");
            }
        }

        #endregion
    }
}
