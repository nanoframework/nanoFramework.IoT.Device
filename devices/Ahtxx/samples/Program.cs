// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Ahtxx;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
//Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

const int I2cBus = 1;
I2cConnectionSettings i2cSettings = new(I2cBus, Aht20.DefaultI2cAddress);
I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);

// For AHT10 or AHT15 use:
// Aht10 sensor = new Aht10(i2cDevice);
// For AHT20 use:
Aht20 sensor = new Aht20(i2cDevice);
while (true)
{
    Debug.WriteLine($"{DateTime.UtcNow}: {sensor.GetTemperature().DegreesCelsius:F1}°C, {sensor.GetHumidity().Percent:F0}%");
    Thread.Sleep(1000);
}
