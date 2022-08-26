// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Mcp7940xx;
using nanoFramework.Hardware.Esp32;

// Setup ESP32 I2C port.
Configuration.SetPinFunction(Gpio.IO21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(Gpio.IO22, DeviceFunction.I2C1_CLOCK);

// Setup Mcp7940m device. 
I2cConnectionSettings i2cSettings = new I2cConnectionSettings(1, Mcp7940m.DefaultI2cAddress);
I2cDevice i2cDevice = new I2cDevice(i2cSettings);

Mcp7940m clock = new Mcp7940m(i2cDevice, ClockSource.ExternalCrystal);
clock.SetTime(DateTime.UtcNow);
clock.StartClock(true);

// Set Alarm 1 to trigger on the 4th minute of every hour.
Mcp7940m.Alarm alarm1 = new Mcp7940m.Alarm(AlarmMatchMode.Minute, minute: 4);
clock.SetAlarm1(alarm1);
clock.EnableAlarm1();
Debug.WriteLine($"Alarm 1 : {clock.GetAlarm1()}");

// Set Alarm 2 to trigger every Wednesday.
Mcp7940m.Alarm alarm2 = new Mcp7940m.Alarm(AlarmMatchMode.DayOfWeek, dayOfWeek: DayOfWeek.Wednesday);
clock.SetAlarm2(alarm2);
clock.EnableAlarm2();
Debug.WriteLine($"Alarm 2 : {clock.GetAlarm2()}");

while (true)
{
    // Get current time.
    DateTime currentTime = clock.GetTime();
    Debug.WriteLine($"Time: {currentTime.ToString("yyyy/MM/dd HH:mm:ss")}");

    // Check if alarm 1 has triggered.
    if (clock.IsTriggeredAlarm1)
    {
        Debug.WriteLine("[ALARM 1]");

        // Clear alarm 1 flag.
        clock.ResetAlarm1();
    }

    // Check if alarm 2 has triggered.
    if (clock.IsTriggeredAlarm2)
    {
        Debug.WriteLine("[ALARM 2]");

        // Clear alarm 2 flag.
        clock.ResetAlarm2();

        // Turn off alarm 2.
        clock.DisableAlarm2();
    }

    // Wait for one second.
    Thread.Sleep(1000);
}
