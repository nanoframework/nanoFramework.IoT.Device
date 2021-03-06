// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Device.Gpio;
using Iot.Device.Seesaw;

const byte AdafruitSeesawSoilSensorI2cAddress = 0x36;
const byte AdafruitSeesawSoilSensorI2cBus = 0x1;

using I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(AdafruitSeesawSoilSensorI2cBus, AdafruitSeesawSoilSensorI2cAddress));
using Seesaw ssDevice = new(i2cDevice);
while (true)
{
    Debug.WriteLine($"Temperature: {ssDevice.GetTemperature()}'C");
    Debug.WriteLine($"Capacitive: {ssDevice.TouchRead(0)}");
    ssDevice.SetGpioPinMode(1, PinMode.Output);
    System.Threading.Tasks.Task.Delay(1000).Wait();
}
