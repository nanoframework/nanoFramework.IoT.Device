// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.SensorHub;

const int I2cBusId = 1;
I2cConnectionSettings connectionSettings = new(I2cBusId, SensorHub.DefaultI2cAddress);
SensorHub sh = new(I2cDevice.Create(connectionSettings));

while (true)
{
    if (sh.TryReadOffBoardTemperature(out var t))
    {
        Debug.WriteLine($"OffBoard temperature {t}");
    }

    if (sh.TryReadBarometerPressure(out var p))
    {
        Debug.WriteLine($"Pressure {p}");
    }

    if (sh.TryReadBarometerTemperature(out var bt))
    {
        Debug.WriteLine($"Barometer temperature {bt}");
    }

    if (sh.TryReadIlluminance(out var l))
    {
        Debug.WriteLine($"Illuminance {l}");
    }

    if (sh.TryReadOnBoardTemperature(out var ot))
    {
        Debug.WriteLine($"OnBoard temperature {ot}");
    }

    if (sh.TryReadRelativeHumidity(out var h))
    {
        Debug.WriteLine($"Relative humidity {h}");
    }

    if (sh.IsMotionDetected)
    {
        Debug.WriteLine("Motion detected");
    }

    Debug.WriteLine("");
    Thread.Sleep(TimeSpan.FromSeconds(1));
}
