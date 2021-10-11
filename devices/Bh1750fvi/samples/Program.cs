// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Bh1750fvi;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
//Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new(busId: 1, (int)I2cAddress.AddPinLow);
using I2cDevice device = I2cDevice.Create(settings);
using Bh1750fvi sensor = new Bh1750fvi(device);
while (true)
{
    Debug.WriteLine($"Illuminance: {sensor.Illuminance}Lux");
    Thread.Sleep(1000);
}
