// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Mcp9808;
using System.Diagnostics;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
//Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new(1, Mcp9808.DefaultI2cAddress);
using I2cDevice device = I2cDevice.Create(settings);

using Mcp9808 sensor = new Mcp9808(device);
while (true)
{
    // read temperature
    Debug.WriteLine($"Temperature: {sensor.Temperature.DegreesCelsius} ℃");
    Debug.WriteLine("");

    Thread.Sleep(1000);
}
