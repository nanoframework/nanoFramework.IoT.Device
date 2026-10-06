// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Max30100;
using nanoFramework.Hardware.Esp32;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new I2cConnectionSettings(1, Max30100.DefaultI2cAddress);
I2cDevice i2cDevice = new I2cDevice(settings);
Max30100 sensor = new Max30100(i2cDevice);

Debug.WriteLine($"MAX30100 part ID: 0x{sensor.PartId:X2}, revision: 0x{sensor.RevisionId:X2}");
if (sensor.PartId != Max30100.ExpectedPartId)
{
    throw new System.InvalidOperationException("MAX30100 was not found on the I2C bus.");
}

sensor.Reset();
sensor.Configure(
    Max30100Mode.SpO2,
    Max30100SampleRate.SamplesPerSecond100,
    Max30100PulseWidth.Microseconds1600,
    Max30100LedCurrent.Milliamps27Point1,
    Max30100LedCurrent.Milliamps27Point1);

while (true)
{
    if (sensor.TryReadSample(out Max30100Reading reading))
    {
        Debug.WriteLine($"IR: {reading.Infrared}, red: {reading.Red}");
    }
    else
    {
        Thread.Sleep(5);
    }
}
