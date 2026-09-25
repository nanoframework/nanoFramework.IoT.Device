// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Lis3Mdl;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

// The on-board LIS3MDL on the B-L475E-IOT01A uses the high I2C address.
// Adjust the bus number if the board firmware maps the sensor bus differently.
I2cConnectionSettings settings = new(2, Lis3Mdl.SecondaryI2cAddress);
using Lis3Mdl sensor = new(I2cDevice.Create(settings));

sensor.MagneticInductionScale = MagneticInductionScale.Scale04G;
sensor.DataRate = DataRate.Rate10Hz;

while (true)
{
    var field = sensor.MagneticField;
    Debug.WriteLine($"X: {field.X:F2} mG, Y: {field.Y:F2} mG, Z: {field.Z:F2} mG");
    Debug.WriteLine($"Temperature: {sensor.Temperature.DegreesCelsius:F1} C");
    Thread.Sleep(1000);
}