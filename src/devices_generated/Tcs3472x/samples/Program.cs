// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Tcs3472x;

Debug.WriteLine("Hello TCS3472x!");
I2cConnectionSettings i2cSettings = new(1, Tcs3472x.DefaultI2cAddress);
using I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);
using Tcs3472x tcs3472X = new(i2cDevice);
while (!Console.KeyAvailable)
{
    Debug.WriteLine($"ID: {tcs3472X.ChipId} Gain: {tcs3472X.Gain} Time to wait: {tcs3472X.IsClearInterrupt}");
    var col = tcs3472X.GetColor();
    Debug.WriteLine($"R: {col.R} G: {col.G} B: {col.B} A: {col.A} Color: {col.Name}");
    Debug.WriteLine($"Valid data: {tcs3472X.IsValidData} Clear Interrupt: {tcs3472X.IsClearInterrupt}");
    Thread.Sleep(1000);
}
