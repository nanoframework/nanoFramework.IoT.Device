using Iot.Device.Common;
using Iot.Device.Mcp7940xx;
using nanoFramework.Hardware.Esp32;
using nanoFramework.TestFramework;
using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace Iot.Device.NFUnitTest
{
    [TestClass]
    public class Mcp7940mTests
    {
        private static I2cDevice _i2cDevice;
        private static Mcp7940m _clock;

        private SpanByte ReadAllBuffers()
        {
            // Read all Mcp7940m registers.
            SpanByte readBuffer = new byte[23];

            _i2cDevice.WriteByte((byte)Register.TimekeepingSecond);
            _i2cDevice.Read(readBuffer);

            return readBuffer;
        }

        [Setup]
        public void SetupMcp7940mTests()
        {
            // Setup ESP32 I2C port.
            Configuration.SetPinFunction(Gpio.IO22, DeviceFunction.I2C1_CLOCK);
            Configuration.SetPinFunction(Gpio.IO21, DeviceFunction.I2C1_DATA);

            // Setup Mcp7940m device. 
            I2cConnectionSettings i2cSettings = new I2cConnectionSettings(1, Mcp7940m.DefaultI2cAddress);
            _i2cDevice = new I2cDevice(i2cSettings);
            _clock = new Mcp7940m(_i2cDevice, ClockSource.ExternalCrystal);

            // Stop clock updating itself during testing.
            _clock.StopClock();

            // Set clock to default state.
            _clock.SetTime(new DateTime(2000, 1, 1, 1, 1, 1));

            Mcp7940m.Alarm alarm1 = new Mcp7940m.Alarm(AlarmMatchMode.Full);
            _clock.SetAlarm1(alarm1);

            Mcp7940m.Alarm alarm2 = new Mcp7940m.Alarm(AlarmMatchMode.Full);
            _clock.SetAlarm2(alarm2);

        }

        // https://stackoverflow.com/questions/42006662/testing-private-static-generic-methods-in-c-sharp
        [TestMethod]
        public void Cannot_Create_With_Null_I2C_Device()
        {
            Assert.Throws(typeof(ArgumentNullException), () => new Mcp7940m(null, ClockSource.ExternalCrystal));
        }

        [TestMethod]
        public void Clock_Is_In_24_Hour_Mode()
        {
            Assert.False(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.TimekeepingHour, (byte)TimekeepingHourRegister.TimeFormat));
        }

        [TestMethod]
        public void SetTime_Only_Changes_Relevant_Registers()
        {
            SpanByte before = ReadAllBuffers();

            _clock.SetTime(new DateTime(2099, 12, 31, 23, 59, 15));

            SpanByte after = ReadAllBuffers();

            // Verify time registers have changed.
            TestHelper.AssertRegistersNotEqual(before, after, Register.TimekeepingSecond);
            TestHelper.AssertRegistersNotEqual(before, after, Register.TimekeepingMinute);
            TestHelper.AssertRegistersNotEqual(before, after, Register.TimekeepingHour);
            TestHelper.AssertRegistersNotEqual(before, after, Register.TimekeepingWeekday);
            TestHelper.AssertRegistersNotEqual(before, after, Register.TimekeepingDay);
            TestHelper.AssertRegistersNotEqual(before, after, Register.TimekeepingMonth);
            TestHelper.AssertRegistersNotEqual(before, after, Register.TimekeepingYear);

            // Verify oscillator input flag has not been altered.
            TestHelper.AssertMaskedRegistersEqual(before, after, Register.TimekeepingSecond, (byte)TimekeepingSecondRegister.OscillatorInputEnabled);

            // Verify oscillator running flag has not been altered.
            TestHelper.AssertMaskedRegistersEqual(before, after, Register.TimekeepingWeekday, (byte)TimekeepingWeekdayRegister.OscillatorRunning);

            // Verify control registers.
            TestHelper.AssertRegistersEqual(before, after, Register.Control);
            TestHelper.AssertRegistersEqual(before, after, Register.OscillatorTrimming);
            TestHelper.AssertRegistersEqual(before, after, Register.EEPROMUnlock);

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

        [TestMethod]
        public void SetAlarmInterruptPolarity_Only_Changes_Relevant_Flag()
        {
            _clock.SetAlarmInterruptPolarity(true);
            SpanByte before = ReadAllBuffers();

            _clock.SetAlarmInterruptPolarity(false);
            SpanByte after = ReadAllBuffers();

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
            TestHelper.AssertRegistersEqual(before, after, Register.EEPROMUnlock);

            // Verify Alarm1 registers.
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Second);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Minute);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Hour);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Day);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm1Month);

            // Verify only AlarmInterruptPolarity flag has been altered on Alarm1.
            TestHelper.AssertMaskedRegistersEqual(before, after, Register.Alarm1Weekday, (byte)~AlarmWeekdayRegister.AlarmInterruptPolarity);
            TestHelper.AssertMaskedRegistersNotEqual(before, after, Register.Alarm1Weekday, (byte)AlarmWeekdayRegister.AlarmInterruptPolarity);

            // Verify Alarm2 registers.
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Second);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Minute);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Hour);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Day);
            TestHelper.AssertRegistersEqual(before, after, Register.Alarm2Month);

            // Verify only non-AlarmInterruptPolarity flags have not been altered on Alarm2.
            TestHelper.AssertMaskedRegistersEqual(before, after, Register.Alarm2Weekday, (byte)(AlarmWeekdayRegister.AlarmMatchModeMask | AlarmWeekdayRegister.AlarmInterrupt | AlarmWeekdayRegister.DayOfWeekMask));
            TestHelper.AssertMaskedRegistersNotEqual(before, after, Register.Alarm2Weekday, (byte)AlarmWeekdayRegister.AlarmInterruptPolarity);
        }

        [TestMethod]
        public void SetAlarmInterruptPolarity_Correctly_Sets_Flag()
        {
            _clock.SetAlarmInterruptPolarity(true);

            // Verify flag has been set.
            Assert.True(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Alarm1Weekday, (byte)AlarmWeekdayRegister.AlarmInterruptPolarity));
            Assert.True(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Alarm1Weekday, (byte)AlarmWeekdayRegister.AlarmInterruptPolarity));

            _clock.SetAlarmInterruptPolarity(false);

            // Verify flag has been cleared.
            Assert.False(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Alarm1Weekday, (byte)AlarmWeekdayRegister.AlarmInterruptPolarity));
            Assert.False(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Alarm1Weekday, (byte)AlarmWeekdayRegister.AlarmInterruptPolarity));
        }

        [TestMethod]
        public void GetAlarmInterruptPolarity_Correctly_Gets_Flag()
        {
            _clock.SetAlarmInterruptPolarity(true);

            // Verify flag matches function return.
            bool interruptPolarity = _clock.GetAlarmInterruptPolarity();
            Assert.Equal(interruptPolarity, RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Alarm1Weekday, (byte)AlarmWeekdayRegister.AlarmInterruptPolarity));
            Assert.Equal(interruptPolarity, RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Alarm1Weekday, (byte)AlarmWeekdayRegister.AlarmInterruptPolarity));

            _clock.SetAlarmInterruptPolarity(false);

            // Verify flag matches function return.
            interruptPolarity = _clock.GetAlarmInterruptPolarity();
            Assert.Equal(interruptPolarity, RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Alarm1Weekday, (byte)AlarmWeekdayRegister.AlarmInterruptPolarity));
            Assert.Equal(interruptPolarity, RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Alarm1Weekday, (byte)AlarmWeekdayRegister.AlarmInterruptPolarity));
        }

        #region Alarm1

        [TestMethod]
        public void SetAlarm1_Correctly_Sets_Registers()
        {
            Mcp7940m.Alarm alarm;

            // ---------------------
            // Second
            // ---------------------

            // Verify match mode is correctly set for second.
            alarm = new Mcp7940m.Alarm(AlarmMatchMode.Second, second: 0);
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)AlarmMatchMode.Second, (byte)RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Weekday) & (byte)AlarmWeekdayRegister.AlarmMatchModeMask);

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
            Assert.Equal((byte)AlarmMatchMode.Minute, (byte)RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Weekday) & (byte)AlarmWeekdayRegister.AlarmMatchModeMask);

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
            Assert.Equal((byte)AlarmMatchMode.Hour, (byte)RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Weekday) & (byte)AlarmWeekdayRegister.AlarmMatchModeMask);

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
            Assert.Equal((byte)AlarmMatchMode.Day, (byte)RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Weekday) & (byte)AlarmWeekdayRegister.AlarmMatchModeMask);

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
            Assert.Equal((byte)AlarmMatchMode.DayOfWeek, (byte)RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Weekday) & (byte)AlarmWeekdayRegister.AlarmMatchModeMask);

            byte before = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Weekday);

            // Verify weekday register is correctly set.
            alarm.DayOfWeek = DayOfWeek.Sunday;
            _clock.SetAlarm1(alarm);
            byte after = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Weekday);
            TestHelper.AssertMaskedRegistersEqual(after, (byte)DayOfWeek.Sunday, (byte)AlarmWeekdayRegister.DayOfWeekMask);
            TestHelper.AssertMaskedRegistersEqual(before, after, (byte)~AlarmWeekdayRegister.DayOfWeekMask);

            alarm.DayOfWeek = DayOfWeek.Monday;
            _clock.SetAlarm1(alarm);
            after = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Weekday);
            TestHelper.AssertMaskedRegistersEqual(after, (byte)DayOfWeek.Monday, (byte)AlarmWeekdayRegister.DayOfWeekMask);
            TestHelper.AssertMaskedRegistersEqual(before, after, (byte)~AlarmWeekdayRegister.DayOfWeekMask);

            alarm.DayOfWeek = DayOfWeek.Tuesday;
            _clock.SetAlarm1(alarm);
            after = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Weekday);
            TestHelper.AssertMaskedRegistersEqual(after, (byte)DayOfWeek.Tuesday, (byte)AlarmWeekdayRegister.DayOfWeekMask);
            TestHelper.AssertMaskedRegistersEqual(before, after, (byte)~AlarmWeekdayRegister.DayOfWeekMask);

            alarm.DayOfWeek = DayOfWeek.Wednesday;
            _clock.SetAlarm1(alarm);
            after = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Weekday);
            TestHelper.AssertMaskedRegistersEqual(after, (byte)DayOfWeek.Wednesday, (byte)AlarmWeekdayRegister.DayOfWeekMask);
            TestHelper.AssertMaskedRegistersEqual(before, after, (byte)~AlarmWeekdayRegister.DayOfWeekMask);

            alarm.DayOfWeek = DayOfWeek.Thursday;
            _clock.SetAlarm1(alarm);
            after = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Weekday);
            TestHelper.AssertMaskedRegistersEqual(after, (byte)DayOfWeek.Thursday, (byte)AlarmWeekdayRegister.DayOfWeekMask);
            TestHelper.AssertMaskedRegistersEqual(before, after, (byte)~AlarmWeekdayRegister.DayOfWeekMask);

            alarm.DayOfWeek = DayOfWeek.Friday;
            _clock.SetAlarm1(alarm);
            after = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Weekday);
            TestHelper.AssertMaskedRegistersEqual(after, (byte)DayOfWeek.Friday, (byte)AlarmWeekdayRegister.DayOfWeekMask);
            TestHelper.AssertMaskedRegistersEqual(before, after, (byte)~AlarmWeekdayRegister.DayOfWeekMask);

            alarm.DayOfWeek = DayOfWeek.Saturday;
            _clock.SetAlarm1(alarm);
            after = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Weekday);
            TestHelper.AssertMaskedRegistersEqual(after, (byte)DayOfWeek.Saturday, (byte)AlarmWeekdayRegister.DayOfWeekMask);
            TestHelper.AssertMaskedRegistersEqual(before, after, (byte)~AlarmWeekdayRegister.DayOfWeekMask);

            // ---------------------
            // Month
            // ---------------------

            // Verify match mode is correctly set for full match.
            alarm = new Mcp7940m.Alarm(AlarmMatchMode.Full, day: 0);
            _clock.SetAlarm1(alarm);
            Assert.Equal((byte)AlarmMatchMode.Full, (byte)RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm1Weekday) & (byte)AlarmWeekdayRegister.AlarmMatchModeMask);

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
        public void SetAlarm1_Only_Changes_Relevant_Registers()
        {
            // Set alarm to known state.
            Mcp7940m.Alarm alarm = new Mcp7940m.Alarm(AlarmMatchMode.Full);
            _clock.SetAlarm1(alarm);

            SpanByte before = ReadAllBuffers();

            alarm = new Mcp7940m.Alarm(AlarmMatchMode.Full, 12, 31, 23, 33, 33, DayOfWeek.Thursday);
            _clock.SetAlarm1(alarm);

            SpanByte after = ReadAllBuffers();

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
            TestHelper.AssertRegistersEqual(before, after, Register.EEPROMUnlock);

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
        public void Enable_And_Disable_Alarm1_Only_Changes_Relevant_Flag()
        {
            _clock.EnableAlarm1();
            SpanByte before = ReadAllBuffers();

            _clock.DisableAlarm1();
            SpanByte after = ReadAllBuffers();

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
            TestHelper.AssertRegistersEqual(before, after, Register.EEPROMUnlock);

            // Verify only Alarm1 Enable flag has been altered.
            TestHelper.AssertMaskedRegistersEqual(before, after, Register.Control, (byte)~ControlRegister.Alarm1InterruptEnabled);
            TestHelper.AssertMaskedRegistersNotEqual(before, after, Register.Control, (byte)ControlRegister.Alarm1InterruptEnabled);

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
        public void Enable_And_Disable_Alarm1_Correctly_Sets_Flag()
        {
            _clock.EnableAlarm1();

            // Verify flag has been set for Alarm1.
            Assert.True(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)ControlRegister.Alarm1InterruptEnabled));

            _clock.DisableAlarm1();

            // Verify flag has been cleared for Alarm1.
            Assert.False(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)ControlRegister.Alarm1InterruptEnabled));
        }

        [TestMethod]
        public void Alarm1IsEnabled_Correctly_Gets_Flag()
        {
            _clock.EnableAlarm1();

            // Verify flag matches function returned state.
            bool isEnabled = _clock.Alarm1IsEnabled();
            Assert.Equal(isEnabled, RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)ControlRegister.Alarm1InterruptEnabled));

            _clock.DisableAlarm1();

            // Verify flag matches function returned state.
            isEnabled = _clock.Alarm1IsEnabled();
            Assert.Equal(isEnabled, RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)ControlRegister.Alarm1InterruptEnabled));
        }

        [TestMethod]
        public void Alarm1IsTriggered_Test()
        {
            Mcp7940m.Alarm alarm = new Mcp7940m.Alarm(AlarmMatchMode.Minute, minute: 34);
            _clock.SetAlarm1(alarm);
            _clock.EnableAlarm1();

            // Clear Alarm1.
            _clock.ResetAlarm1();
            Assert.False(_clock.Alarm1IsTriggered());

            // Set time to trigger alarm.
            _clock.SetTime(new DateTime(2022, 8, 15, 23, 33, 59));

            // Start clock.
            _clock.StartClock(true);

            // Wait for one second for clock to update its second register.
            Thread.Sleep(1000);

            Assert.True(_clock.Alarm1IsTriggered());

            _clock.StopClock();
        }

        #endregion

        #region Alarm2

        [TestMethod]
        public void SetAlarm2_Correctly_Sets_Registers()
        {
            Mcp7940m.Alarm alarm;

            // ---------------------
            // Second
            // ---------------------

            // Verify match mode is correctly set for second.
            alarm = new Mcp7940m.Alarm(AlarmMatchMode.Second, second: 0);
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)AlarmMatchMode.Second, (byte)RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Weekday) & (byte)AlarmWeekdayRegister.AlarmMatchModeMask);

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
            Assert.Equal((byte)AlarmMatchMode.Minute, (byte)RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Weekday) & (byte)AlarmWeekdayRegister.AlarmMatchModeMask);

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
            Assert.Equal((byte)AlarmMatchMode.Hour, (byte)RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Weekday) & (byte)AlarmWeekdayRegister.AlarmMatchModeMask);

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
            Assert.Equal((byte)AlarmMatchMode.Day, (byte)RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Weekday) & (byte)AlarmWeekdayRegister.AlarmMatchModeMask);

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
            Assert.Equal((byte)AlarmMatchMode.DayOfWeek, (byte)RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Weekday) & (byte)AlarmWeekdayRegister.AlarmMatchModeMask);

            byte before = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Weekday);

            // Verify weekday register is correctly set.
            alarm.DayOfWeek = DayOfWeek.Sunday;
            _clock.SetAlarm2(alarm);
            byte after = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Weekday);
            TestHelper.AssertMaskedRegistersEqual(after, (byte)DayOfWeek.Sunday, (byte)AlarmWeekdayRegister.DayOfWeekMask);
            TestHelper.AssertMaskedRegistersEqual(before, after, (byte)~AlarmWeekdayRegister.DayOfWeekMask);

            alarm.DayOfWeek = DayOfWeek.Monday;
            _clock.SetAlarm2(alarm);
            after = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Weekday);
            TestHelper.AssertMaskedRegistersEqual(after, (byte)DayOfWeek.Monday, (byte)AlarmWeekdayRegister.DayOfWeekMask);
            TestHelper.AssertMaskedRegistersEqual(before, after, (byte)~AlarmWeekdayRegister.DayOfWeekMask);

            alarm.DayOfWeek = DayOfWeek.Tuesday;
            _clock.SetAlarm2(alarm);
            after = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Weekday);
            TestHelper.AssertMaskedRegistersEqual(after, (byte)DayOfWeek.Tuesday, (byte)AlarmWeekdayRegister.DayOfWeekMask);
            TestHelper.AssertMaskedRegistersEqual(before, after, (byte)~AlarmWeekdayRegister.DayOfWeekMask);

            alarm.DayOfWeek = DayOfWeek.Wednesday;
            _clock.SetAlarm2(alarm);
            after = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Weekday);
            TestHelper.AssertMaskedRegistersEqual(after, (byte)DayOfWeek.Wednesday, (byte)AlarmWeekdayRegister.DayOfWeekMask);
            TestHelper.AssertMaskedRegistersEqual(before, after, (byte)~AlarmWeekdayRegister.DayOfWeekMask);

            alarm.DayOfWeek = DayOfWeek.Thursday;
            _clock.SetAlarm2(alarm);
            after = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Weekday);
            TestHelper.AssertMaskedRegistersEqual(after, (byte)DayOfWeek.Thursday, (byte)AlarmWeekdayRegister.DayOfWeekMask);
            TestHelper.AssertMaskedRegistersEqual(before, after, (byte)~AlarmWeekdayRegister.DayOfWeekMask);

            alarm.DayOfWeek = DayOfWeek.Friday;
            _clock.SetAlarm2(alarm);
            after = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Weekday);
            TestHelper.AssertMaskedRegistersEqual(after, (byte)DayOfWeek.Friday, (byte)AlarmWeekdayRegister.DayOfWeekMask);
            TestHelper.AssertMaskedRegistersEqual(before, after, (byte)~AlarmWeekdayRegister.DayOfWeekMask);

            alarm.DayOfWeek = DayOfWeek.Saturday;
            _clock.SetAlarm2(alarm);
            after = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Weekday);
            TestHelper.AssertMaskedRegistersEqual(after, (byte)DayOfWeek.Saturday, (byte)AlarmWeekdayRegister.DayOfWeekMask);
            TestHelper.AssertMaskedRegistersEqual(before, after, (byte)~AlarmWeekdayRegister.DayOfWeekMask);

            // ---------------------
            // Month
            // ---------------------

            // Verify match mode is correctly set for full match.
            alarm = new Mcp7940m.Alarm(AlarmMatchMode.Full, day: 0);
            _clock.SetAlarm2(alarm);
            Assert.Equal((byte)AlarmMatchMode.Full, (byte)RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Alarm2Weekday) & (byte)AlarmWeekdayRegister.AlarmMatchModeMask);

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
        public void SetAlarm2_Only_Changes_Relevant_Registers()
        {
            // Set alarm to known state.
            Mcp7940m.Alarm alarm = new Mcp7940m.Alarm(AlarmMatchMode.Full);
            _clock.SetAlarm2(alarm);

            SpanByte before = ReadAllBuffers();

            alarm = new Mcp7940m.Alarm(AlarmMatchMode.Full, 12, 31, 23, 33, 33, DayOfWeek.Thursday);
            _clock.SetAlarm2(alarm);

            SpanByte after = ReadAllBuffers();

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
            TestHelper.AssertRegistersEqual(before, after, Register.EEPROMUnlock);

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
        public void Enable_And_Disable_Alarm2_Only_Changes_Relevant_Flag()
        {
            _clock.EnableAlarm2();
            SpanByte before = ReadAllBuffers();

            _clock.DisableAlarm2();
            SpanByte after = ReadAllBuffers();

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
            TestHelper.AssertRegistersEqual(before, after, Register.EEPROMUnlock);

            // Verify only Alarm2 Enable flag has been altered.
            TestHelper.AssertMaskedRegistersEqual(before, after, Register.Control, (byte)~ControlRegister.Alarm2InterruptEnabled);
            TestHelper.AssertMaskedRegistersNotEqual(before, after, Register.Control, (byte)ControlRegister.Alarm2InterruptEnabled);

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
        public void Enable_And_Disable_Alarm2_Correctly_Sets_Flag()
        {
            _clock.EnableAlarm2();

            // Verify flag has been set for Alarm2.
            Assert.True(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)ControlRegister.Alarm2InterruptEnabled));

            _clock.DisableAlarm2();

            // Verify flag has been cleared for Alarm2.
            Assert.False(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)ControlRegister.Alarm2InterruptEnabled));
        }

        [TestMethod]
        public void Alarm2IsEnabled_Correctly_Gets_Flag()
        {
            _clock.EnableAlarm2();

            // Verify flag matches function returned state.
            bool isEnabled = _clock.Alarm2IsEnabled();
            Assert.Equal(isEnabled, RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)ControlRegister.Alarm2InterruptEnabled));

            _clock.DisableAlarm2();

            // Verify flag matches function returned state for both Alarm2 and Alarm2.
            isEnabled = _clock.Alarm2IsEnabled();
            Assert.Equal(isEnabled, RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)ControlRegister.Alarm2InterruptEnabled));
        }

        [TestMethod]
        public void Alarm2IsTriggered_Test()
        {
            Mcp7940m.Alarm alarm = new Mcp7940m.Alarm(AlarmMatchMode.Minute, minute: 34);
            _clock.SetAlarm2(alarm);
            _clock.EnableAlarm2();

            // Clear Alarm2.
            _clock.ResetAlarm2();
            Assert.False(_clock.Alarm2IsTriggered());

            // Set time to trigger alarm.
            _clock.SetTime(new DateTime(2022, 8, 15, 23, 33, 59));

            // Start clock.
            _clock.StartClock(true);

            // Wait for one second for clock to update its second register.
            Thread.Sleep(1000);

            Assert.True(_clock.Alarm2IsTriggered());

            _clock.StopClock();
        }

        #endregion

        #region  General Purpose Output

        [TestMethod]
        public void Enable_And_Disable_GeneralPurposeOutput_Only_Changes_Relevant_Flag()
        {
            _clock.SetGeneralPurposeOutputHigh();
            SpanByte before = ReadAllBuffers();

            _clock.SetGeneralPurposeOutputLow();
            SpanByte after = ReadAllBuffers();

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
            TestHelper.AssertRegistersEqual(before, after, Register.EEPROMUnlock);

            // Verify only GeneralPurposeOutput Enable flag has been altered.
            TestHelper.AssertMaskedRegistersEqual(before, after, Register.Control, (byte)~ControlRegister.GeneralPurposeOutput);
            TestHelper.AssertMaskedRegistersNotEqual(before, after, Register.Control, (byte)ControlRegister.GeneralPurposeOutput);

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
        public void Enable_And_Disable_GeneralPurposeOutput_Correctly_Sets_Flag()
        {
            _clock.SetGeneralPurposeOutputHigh();

            // Verify flag has been set for GeneralPurposeOutput.
            Assert.True(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)ControlRegister.GeneralPurposeOutput));

            _clock.SetGeneralPurposeOutputLow();

            // Verify flag has been cleared for GeneralPurposeOutput.
            Assert.False(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)ControlRegister.GeneralPurposeOutput));
        }

        [TestMethod]
        public void GeneralPurposeOutputIsEnabled_Correctly_Gets_Flag()
        {
            _clock.SetGeneralPurposeOutputHigh();

            // Verify flag matches function returned state.
            bool isEnabled = _clock.GeneralPurposeOutputIsHigh();
            Assert.Equal(isEnabled, RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)ControlRegister.GeneralPurposeOutput));

            _clock.SetGeneralPurposeOutputLow();

            // Verify flag matches function returned state.
            isEnabled = _clock.GeneralPurposeOutputIsHigh();
            Assert.Equal(isEnabled, RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)ControlRegister.GeneralPurposeOutput));
        }

        #endregion

        #region Square Wave

        [TestMethod]
        public void Enable_And_Disable_SquareWaveOutput_Only_Changes_Relevant_Flag()
        {
            _clock.EnableSquareWaveOutput();
            SpanByte before = ReadAllBuffers();

            _clock.DisableSquareWaveOutput();
            SpanByte after = ReadAllBuffers();

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
            TestHelper.AssertRegistersEqual(before, after, Register.EEPROMUnlock);

            // Verify only SquareWaveOutput Enable flag has been altered.
            TestHelper.AssertMaskedRegistersEqual(before, after, Register.Control, (byte)~ControlRegister.SquareWaveOutput);
            TestHelper.AssertMaskedRegistersNotEqual(before, after, Register.Control, (byte)ControlRegister.SquareWaveOutput);

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
        public void Enable_And_Disable_SquareWaveOutput_Correctly_Sets_Flag()
        {
            _clock.EnableSquareWaveOutput();

            // Verify flag has been set for SquareWaveOutput.
            Assert.True(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)ControlRegister.SquareWaveOutput));

            _clock.DisableSquareWaveOutput();

            // Verify flag has been cleared for SquareWaveOutput.
            Assert.False(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)ControlRegister.SquareWaveOutput));
        }

        [TestMethod]
        public void SquareWaveOutputIsEnabled_Correctly_Gets_Flag()
        {
            _clock.EnableSquareWaveOutput();

            // Verify flag matches function returned state.
            bool isEnabled = _clock.SquareWaveOutputIsEnabled();
            Assert.Equal(isEnabled, RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)ControlRegister.SquareWaveOutput));

            _clock.DisableSquareWaveOutput();

            // Verify flag matches function returned state.
            isEnabled = _clock.SquareWaveOutputIsEnabled();
            Assert.Equal(isEnabled, RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)ControlRegister.SquareWaveOutput));
        }

        public void SetSquareWaveFrequency_Only_Changes_Relevant_Flag()
        {
            _clock.SetSquareWaveFrequency(SquareWaveFrequency.Frequency1Hz);
            SpanByte before = ReadAllBuffers();

            _clock.SetSquareWaveFrequency(SquareWaveFrequency.Frequency32kHz);
            SpanByte after = ReadAllBuffers();

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
            TestHelper.AssertRegistersEqual(before, after, Register.EEPROMUnlock);

            // Verify only SquareWaveOutput Enable flag has been altered.
            TestHelper.AssertMaskedRegistersEqual(before, after, Register.Control, (byte)~ControlRegister.SquareWaveFrequencyMask);
            TestHelper.AssertMaskedRegistersNotEqual(before, after, Register.Control, (byte)ControlRegister.SquareWaveFrequencyMask);

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
        public void SetSquareWaveFrequency_Correctly_Sets_Flag()
        {
            _clock.SetSquareWaveFrequency(SquareWaveFrequency.Frequency1Hz);
            TestHelper.AssertMaskedRegistersEqual((byte)SquareWaveFrequency.Frequency1Hz, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Control), (byte)ControlRegister.SquareWaveFrequencyMask);

            _clock.SetSquareWaveFrequency(SquareWaveFrequency.Frequency4kHz);
            TestHelper.AssertMaskedRegistersEqual((byte)SquareWaveFrequency.Frequency4kHz, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Control), (byte)ControlRegister.SquareWaveFrequencyMask);

            _clock.SetSquareWaveFrequency(SquareWaveFrequency.Frequency8kHz);
            TestHelper.AssertMaskedRegistersEqual((byte)SquareWaveFrequency.Frequency8kHz, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Control), (byte)ControlRegister.SquareWaveFrequencyMask);

            _clock.SetSquareWaveFrequency(SquareWaveFrequency.Frequency32kHz);
            TestHelper.AssertMaskedRegistersEqual((byte)SquareWaveFrequency.Frequency32kHz, RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Control), (byte)ControlRegister.SquareWaveFrequencyMask);
        }


        [TestMethod]
        public void GetSquareWaveFrequency_Correctly_Gets_Flag()
        {
            _clock.SetSquareWaveFrequency(SquareWaveFrequency.Frequency1Hz);
            Assert.Equal((byte)SquareWaveFrequency.Frequency1Hz, (byte)_clock.GetSquareWaveFrequency());

            _clock.SetSquareWaveFrequency(SquareWaveFrequency.Frequency4kHz);
            Assert.Equal((byte)SquareWaveFrequency.Frequency4kHz, (byte)_clock.GetSquareWaveFrequency());

            _clock.SetSquareWaveFrequency(SquareWaveFrequency.Frequency8kHz);
            Assert.Equal((byte)SquareWaveFrequency.Frequency8kHz, (byte)_clock.GetSquareWaveFrequency());

            _clock.SetSquareWaveFrequency(SquareWaveFrequency.Frequency32kHz);
            Assert.Equal((byte)SquareWaveFrequency.Frequency32kHz, (byte)_clock.GetSquareWaveFrequency());
        }

        #endregion

        #region Clock

        [TestMethod]
        public void SetClockSource_Only_Changes_Relevant_Flag()
        {
            _clock.SetClockSource(ClockSource.ExternalCrystal);
            SpanByte before = ReadAllBuffers();

            _clock.SetClockSource(ClockSource.ExternalClockInput);
            SpanByte after = ReadAllBuffers();

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
            TestHelper.AssertRegistersEqual(before, after, Register.EEPROMUnlock);

            // Verify only Alarm1 Enable flag has been altered.
            TestHelper.AssertMaskedRegistersEqual(before, after, Register.Control, (byte)~ControlRegister.ExternalClockInput);
            TestHelper.AssertMaskedRegistersNotEqual(before, after, Register.Control, (byte)ControlRegister.ExternalClockInput);

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
        public void SetClockSource_Correctly_Sets_Flag()
        {
            _clock.SetClockSource(ClockSource.ExternalClockInput);
            Assert.True(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)ControlRegister.ExternalClockInput));

            _clock.SetClockSource(ClockSource.ExternalCrystal);
            Assert.False(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)ControlRegister.ExternalClockInput));
        }

        [TestMethod]
        public void GetClockSource_Correctly_Gets_Flag()
        {
            _clock.SetClockSource(ClockSource.ExternalClockInput);

            // Verify flag matches function returned state.
            ClockSource clockSource = _clock.GetClockSource();
            Assert.True(clockSource == ClockSource.ExternalClockInput);

            _clock.SetClockSource(ClockSource.ExternalCrystal);

            // Verify flag matches function returned state.
            clockSource = _clock.GetClockSource();
            Assert.True(clockSource == ClockSource.ExternalCrystal);
        }

        public void Start_And_Stop_Clock_Correctly_Sets_Flag()
        {
            _clock.StartClock();
            Assert.True(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.TimekeepingSecond, (byte)TimekeepingSecondRegister.OscillatorInputEnabled));

            _clock.StopClock();
            Assert.False(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.TimekeepingSecond, (byte)TimekeepingSecondRegister.OscillatorInputEnabled));
        }

        [TestMethod]
        public void Start_And_Stop_Clock_Only_Changes_Relevant_Flag()
        {
            _clock.StartClock();
            SpanByte before = ReadAllBuffers();

            _clock.StopClock();
            SpanByte after = ReadAllBuffers();

            // Verify time keeping registers.
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingMinute);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingHour);
            TestHelper.AssertMaskedRegistersEqual(before, after, Register.TimekeepingWeekday, unchecked((byte)~TimekeepingWeekdayRegister.OscillatorRunning));
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingDay);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingMonth);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingYear);

            // Verify only Alarm1 Enable flag has been altered.
            TestHelper.AssertMaskedRegistersEqual(before, after, Register.TimekeepingSecond, unchecked((byte)~TimekeepingSecondRegister.OscillatorInputEnabled));
            TestHelper.AssertMaskedRegistersNotEqual(before, after, Register.TimekeepingSecond, (byte)TimekeepingSecondRegister.OscillatorInputEnabled);

            // Verify control registers.
            TestHelper.AssertRegistersEqual(before, after, Register.OscillatorTrimming);
            TestHelper.AssertRegistersEqual(before, after, Register.EEPROMUnlock);
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
        public void ClockIsHalted_Correctly_Gets_Flag()
        {
            _clock.StartClock();

            // Verify flag matches function returned state.
            Assert.False(_clock.ClockIsHalted());

            _clock.StopClock();

            // Verify flag matches function returned state.
            Assert.True(_clock.ClockIsHalted());
        }

        #endregion

        #region SRAM

        [TestMethod]
        public void ConvertAddressToSRAM_Works_Correctly()
        {
            for (byte address = 0; address < 63; address++)
            {
                Assert.Equal(0x20 + address, _clock.ConvertAddressToSRAM(address));
            }
        }

        [TestMethod]
        public void ConvertAddressToSRAM_Throws_Exception_When_Address_Out_Of_Range()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => _clock.ConvertAddressToSRAM(64));
        }

        [TestMethod]
        public void Read_And_Write_Byte_From_SRAM_Works_Correctly()
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
