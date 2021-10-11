// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Lm75;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
//Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new(1, Lm75.DefaultI2cAddress);
using I2cDevice device = I2cDevice.Create(settings);

using Lm75 sensor = new(device);
while (true)
{
    // read temperature
    Debug.WriteLine($"Temperature: {sensor.Temperature.DegreesCelsius} ℃");
    Debug.WriteLine("");

    Thread.Sleep(1000);
}
