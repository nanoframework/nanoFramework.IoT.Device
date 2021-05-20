// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Numerics;
using System.Threading;
using System.Device.Spi;
using Iot.Device.Adxl345;
using System.Diagnostics;

SpiConnectionSettings settings = new(0, 0)
{
    ClockFrequency = Adxl345.SpiClockFrequency,
    Mode = Adxl345.SpiMode
};

using SpiDevice device = SpiDevice.Create(settings);
// set gravity measurement range ±4G
using Adxl345 sensor = new Adxl345(device, GravityRange.Range04);
while (true)
{
    // read data
    Vector3 data = sensor.Acceleration;

    Debug.WriteLine($"X: {data.X.ToString("0.00")} g");
    Debug.WriteLine($"Y: {data.Y.ToString("0.00")} g");
    Debug.WriteLine($"Z: {data.Z.ToString("0.00")} g");
    Debug.WriteLine("");

    // wait for 500ms
    Thread.Sleep(500);
}
