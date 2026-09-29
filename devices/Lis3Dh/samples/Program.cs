// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Lis3DhAccelerometer;
using nanoFramework.Hardware.Esp32;
using System.Device.I2c;
using System.Diagnostics;
using System.Numerics;
using System.Threading;

Configuration.SetPinFunction(Gpio.IO21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(Gpio.IO22, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new(1, Lis3Dh.DefaultI2cAddress);
using I2cDevice i2cDevice = I2cDevice.Create(settings);
using Lis3Dh accelerometer = Lis3Dh.Create(i2cDevice, DataRate.DataRate10Hz);

Debug.WriteLine("Orient the sensor so that two axes are close to 0 g.");
Debug.WriteLine("The remaining axis should be close to 1 g or -1 g.");

while (true)
{
    Vector3 acceleration = accelerometer.Acceleration;
    Debug.WriteLine($"X: {acceleration.X:F4} g, Y: {acceleration.Y:F4} g, Z: {acceleration.Z:F4} g");
    Thread.Sleep(100);
}
