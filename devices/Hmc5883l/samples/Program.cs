// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Hmc5883l;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
//Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new(1, Hmc5883l.DefaultI2cAddress);
using I2cDevice device = I2cDevice.Create(settings);
using Hmc5883l sensor = new(device);
while (true)
{
    // read heading
    Debug.WriteLine($"Heading: {sensor.Heading.ToString("0.00")} °");

    var status = sensor.DeviceStatus;
    Debug.Write("Statuses: ");
    switch (status)
    {
        case Status.Ready:
            Debug.Write($"Ready ");
            break;
        case Status.Lock:
            Debug.Write($"Lock ");
            break;
        case Status.RegulatorEnabled:
            Debug.Write($"RegulatorEnabled ");
            break;
        default:
            break;
    }

    Debug.WriteLine("");
    Debug.WriteLine("");

    // wait for a second
    Thread.Sleep(1000);
}
