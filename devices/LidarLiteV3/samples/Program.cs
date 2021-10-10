// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Device.I2c;
using Iot.Device.DistanceSensor;
using UnitsNet;
using System.Diagnostics;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
//Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

using (var llv3 = new LidarLiteV3(CreateI2cDevice()))
{
    // Take 10 measurements, each one second apart.
    for (int i = 0; i < 10; i++)
    {
        Length currentDistance = llv3.MeasureDistance();
        Debug.WriteLine($"Current Distance: {currentDistance.Centimeters} cm");
        Thread.Sleep(1000);
    }
}

I2cDevice CreateI2cDevice()
{
    var settings = new I2cConnectionSettings(1, LidarLiteV3.DefaultI2cAddress);
    return I2cDevice.Create(settings);
}
