// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Bh1745;

// bus id on the raspberry pi 3
const int busId = 1;

// create device
I2cConnectionSettings i2cSettings = new(busId, Bh1745.DefaultI2cAddress);
using I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);
using Bh1745 i2cBh1745 = new Bh1745(i2cDevice);
// wait for first measurement
Thread.Sleep(i2cBh1745.MeasurementTimeAsTimeSpan());

while (true)
{
    var color = i2cBh1745.GetCompensatedColor();
    Debug.WriteLine($"RGB color read: #{color.R:X2}{color.G:X2}{color.B:X2}");
    Debug.WriteLine($"Raw illumination value: {i2cBh1745.ReadClearDataRegister()}");

    Thread.Sleep(i2cBh1745.MeasurementTimeAsTimeSpan());
}
