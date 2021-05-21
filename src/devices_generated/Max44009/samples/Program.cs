// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Max44009;

I2cConnectionSettings settings = new(1, Max44009.DefaultI2cAddress);
using I2cDevice device = I2cDevice.Create(settings);

// integration time is 100ms
using Max44009 sensor = new(device, IntegrationTime.Time100);
while (true)
{
    // read illuminance
    Debug.WriteLine($"Illuminance: {sensor.Illuminance}Lux");
    Debug.WriteLine();

    Thread.Sleep(1000);
}
