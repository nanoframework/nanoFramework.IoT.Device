// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Lsm6Dsl;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

// The on-board LSM6DSL on the B-L475E-IOT01A uses the low I2C address.
// Adjust the bus number if the board firmware maps the sensor bus differently.
I2cConnectionSettings settings = new(2, Lsm6Dsl.DefaultI2cAddress);
using Lsm6Dsl sensor = new(I2cDevice.Create(settings));

sensor.AccelerationScale = AccelerationScale.Scale02G;
sensor.AngularRateScale = AngularRateScale.Scale0250Dps;

while (true)
{
    var acceleration = sensor.Acceleration;
    var angularRate = sensor.AngularRate;
    Debug.WriteLine($"Acceleration: X={acceleration.X:F3} g, Y={acceleration.Y:F3} g, Z={acceleration.Z:F3} g");
    Debug.WriteLine($"Angular rate: X={angularRate.X:F2} dps, Y={angularRate.Y:F2} dps, Z={angularRate.Z:F2} dps");
    Debug.WriteLine($"Temperature: {sensor.Temperature.DegreesCelsius:F1} C");
    Thread.Sleep(1000);
}