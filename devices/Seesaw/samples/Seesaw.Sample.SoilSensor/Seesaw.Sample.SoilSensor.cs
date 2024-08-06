// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Device.Gpio;
using System.Threading;
using Iot.Device.Seesaw;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus.
// using nanoFramework.Hardware.Esp32;
// Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
// Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

const byte AdafruitSeesawSoilSensorI2cAddress = 0x36;
const byte AdafruitSeesawSoilSensorI2cBus = 0x1;

using I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(AdafruitSeesawSoilSensorI2cBus, AdafruitSeesawSoilSensorI2cAddress));
using Seesaw ssDevice = new(i2cDevice);
while (true)
{
    Console.WriteLine($"Temperature: {ssDevice.GetTemperature()}'C");
    Console.WriteLine($"Capacitive: {ssDevice.TouchRead(0)}");
    ssDevice.SetGpioPinMode(1, PinMode.Output);
    Thread.Sleep(1000);
}
