// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Common;
using Iot.Device.Mcp7940xx;
using nanoFramework.Hardware.Esp32;
using nanoFramework.TestFramework;
using System;
using System.Device.I2c;
using System.Diagnostics;

namespace Iot.Device.NFUnitTest
{
    [TestClass]
    public class Mcp7940nTests
    {
        static I2cDevice _i2cDevice;
        static Mcp7940n _clock;

        private SpanByte ReadAllBuffers()
        {
            // Read all Mcp7940n registers.
            SpanByte readBuffer = new byte[31];

            _i2cDevice.WriteByte((byte)Register.TimekeepingSecond);
            _i2cDevice.Read(readBuffer);

            return readBuffer;
        }

        [Setup]
        public void SetupMcp7940nTests()
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
                _clock = new Mcp7940n(_i2cDevice, ClockSource.ExternalCrystal);

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

        #region External Battery Backup

        [TestMethod]
        public void ExternalBatteryBackup_EnableExternalBatteryBackup_And_DisableExternalBatteryBackup_Only_Changes_Relevant_Flag()
        {
            _clock.EnableExternalBatteryBackup();
            SpanByte before = ReadAllBuffers();

            _clock.DisableExternalBatteryBackup();
            SpanByte after = ReadAllBuffers();

            // Verify time keeping registers.
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingSecond);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingMinute);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingHour);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingDay);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingMonth);
            TestHelper.AssertRegistersEqual(before, after, Register.TimekeepingYear);

            // Verify only Alarm1 Enable flag has been altered.
            TestHelper.AssertMaskedRegistersEqual(before, after, Register.TimekeepingWeekday, unchecked((byte)~RegisterMask.ExternalBatteryBackupEnabledMask));
            TestHelper.AssertMaskedRegistersNotEqual(before, after, Register.TimekeepingWeekday, (byte)RegisterMask.ExternalBatteryBackupEnabledMask);

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
        public void ExternalBatteryBackup_EnableExternalBatteryBackup_And_DisableExternalBatteryBackup_Correctly_Sets_Flag()
        {
            _clock.EnableExternalBatteryBackup();

            // Verify flag has been set.
            Assert.True(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.TimekeepingWeekday, (byte)RegisterMask.ExternalBatteryBackupEnabledMask));

            _clock.DisableExternalBatteryBackup();

            // Verify flag has been cleared.
            Assert.False(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.TimekeepingWeekday, (byte)RegisterMask.ExternalBatteryBackupEnabledMask));
        }

        [TestMethod]
        public void ExternalBatteryBackup_IsEnabledExternalBatteryBackup_Property_Correctly_Gets_Flag()
        {
            _clock.EnableExternalBatteryBackup();

            // Verify flag matches function returned state.
            bool isEnabled = _clock.IsEnabledExternalBatteryBackup;
            Assert.Equal(isEnabled, RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.TimekeepingWeekday, (byte)RegisterMask.ExternalBatteryBackupEnabledMask));

            _clock.DisableExternalBatteryBackup();

            // Verify flag matches function returned state.
            isEnabled = _clock.IsEnabledExternalBatteryBackup;
            Assert.Equal(isEnabled, RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.TimekeepingWeekday, (byte)RegisterMask.ExternalBatteryBackupEnabledMask));
        }

        #endregion
    }
}
