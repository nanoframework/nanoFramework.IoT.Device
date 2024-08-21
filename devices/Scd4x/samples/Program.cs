// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Scd4x;
using nanoFramework.Hardware.Esp32;
using UnitsNet;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
Configuration.SetPinFunction(23, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new(1, Scd4x.I2cDefaultAddress);
using I2cDevice device = I2cDevice.Create(settings);
using Scd4x sensor = new(device);
sensor.StopPeriodicMeasurement();
var serialNumber = sensor.GetSerialNumber();
Console.WriteLine($"Serial number: {serialNumber}");
var offset = sensor.GetTemperatureOffset();
Console.WriteLine($"Temperature offset: {offset.DegreesCelsius}");
sensor.SetTemperatureOffset(Temperature.FromDegreesCelsius(4));
offset = sensor.GetTemperatureOffset();
Console.WriteLine($"New temperature offset: {offset.DegreesCelsius}");

sensor.StartPeriodicMeasurement();
while (true)
{
    if (!sensor.IsDataReady())
    {
        Thread.Sleep(1000);
        continue;
    }

    var data = sensor.ReadData();
    Console.WriteLine($"Temperature: {data.Temperature.DegreesCelsius} \u00B0C");
    Console.WriteLine($"Relative humidity: {data.RelativeHumidity.Percent} %RH");
    Console.WriteLine($"CO2: {data.CO2} PPM");
}
