// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Ds1621;
using nanoFramework.Hardware.Esp32;
using UnitsNet;

string alarmState;

// Setup ESP32 I2C port.
Configuration.SetPinFunction(Gpio.IO21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(Gpio.IO22, DeviceFunction.I2C1_CLOCK);

// Setup Ds1621 device.
I2cConnectionSettings i2cSettings = new I2cConnectionSettings(1, Ds1621.DefaultI2cAddress);
I2cDevice i2cDevice = new I2cDevice(i2cSettings);

Ds1621 thermometer = new Ds1621(i2cDevice, MeasurementMode.Single);

// Set temperature alarms.
thermometer.LowTemperatureAlarm = Temperature.FromDegreesFahrenheit(65);
thermometer.HighTemperatureAlarm = Temperature.FromDegreesFahrenheit(80);

while (true)
{
    // Start temperature conversion.
    thermometer.MeasureTemperature();

    // Wait for temperature conversion to complete.
    while (thermometer.IsMeasuringTemperature)
    {
        Thread.Sleep(10);
    }

    Temperature temperature = thermometer.GetTemperature();

    // Check temperature alarm states.
    if (thermometer.HasLowTemperatureAlarm)
    {
        alarmState = "[Low Temperature]";
    }
    else if (thermometer.HasHighTemperatureAlarm)
    {
        alarmState = "[High Temperature]";
    }
    else
    {
        alarmState = string.Empty;
    }

    Debug.WriteLine($"{DateTime.UtcNow} : {temperature.DegreesCelsius:F1}°C / {temperature.DegreesFahrenheit:F1}°F {alarmState}");

    Thread.Sleep(1000);
}
