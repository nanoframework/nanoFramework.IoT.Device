// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Pmsa003i;
using nanoFramework.Hardware.Esp32;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

Configuration.SetPinFunction(6, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(7, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new I2cConnectionSettings(1, Pmsa003i.DefaultI2cAddress);
I2cDevice i2cDevice = new I2cDevice(settings);
Pmsa003i sensor = new Pmsa003i(i2cDevice);

while (true)
{
    PmsReading reading = sensor.Read();
    Debug.WriteLine($"PM1.0: {reading.Pm1Atmospheric} ug/m3");
    Debug.WriteLine($"PM2.5: {reading.Pm2Point5Atmospheric} ug/m3");
    Debug.WriteLine($"PM10: {reading.Pm10Atmospheric} ug/m3");
    Debug.WriteLine($"Particles >0.3um: {reading.ParticlesLargerThan0Point3Micrometers}");
    Debug.WriteLine($"Particles >0.5um: {reading.ParticlesLargerThan0Point5Micrometers}");
    Debug.WriteLine($"Version: {reading.Version}, error: {reading.ErrorCode}");
    Debug.WriteLine($"---");

    Thread.Sleep(1000);
}