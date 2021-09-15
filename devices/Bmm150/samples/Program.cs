// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using System.Numerics;
using Iot.Device.Magnetometer;
using nanoFramework.Hardware.Esp32;

// The I2C pins 21 and 22 in the sample below are ESP32 specific and may differ from other platforms.
// Please double check your device datasheet.
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);

I2cConnectionSettings mpui2CConnectionSettingmpus = new(1, Bmm150.DefaultI2cAddress);

using Bmm150 bmm150 = new Bmm150(I2cDevice.Create(mpui2CConnectionSettingmpus));

Debug.WriteLine($"move it now...");
bmm150.bmm150_calibrate();
Debug.WriteLine($"bmm150_calibrate done!");

while (true)
{
    Vector3 magne = bmm150.ReadMagnetometer(true, TimeSpan.FromMilliseconds(11));

    //var head_dir = Math.Atan2(magne.X - offset.X, magne.Y - offset.Y) * 180.0 / Math.PI;

    // Debug.WriteLine($"head_dir: {head_dir}");
    Debug.WriteLine($"Mag data: {magne.X,15},{magne.Y,15},{magne.Z,15}");

    Thread.Sleep(100);
}
