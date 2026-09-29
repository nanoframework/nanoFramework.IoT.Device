// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

using Iot.Device.Vl53L1X;

using nanoFramework.Hardware.Esp32;

Configuration.SetPinFunction(Gpio.IO21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(Gpio.IO22, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new(1, Vl53L1X.DefaultI2cAddress);
using I2cDevice i2cDevice = I2cDevice.Create(settings);
using Vl53L1X sensor = new(i2cDevice);

Debug.WriteLine($"Sensor ID: 0x{sensor.SensorId:X4}");
sensor.Precision = Precision.Short;

while (true)
{
    try
    {
        Debug.WriteLine($"Distance: {sensor.Distance.Millimeters} mm");
        Debug.WriteLine($"Range status: {sensor.RangeStatus}");
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"Exception: {ex.Message}");
    }

    Thread.Sleep(500);
}
