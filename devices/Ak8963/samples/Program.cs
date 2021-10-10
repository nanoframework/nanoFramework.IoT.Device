// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using System.Numerics;
using Iot.Device.Magnetometer;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
//Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings mpui2CConnectionSettingmpus = new(1, Ak8963.DefaultI2cAddress);
using Ak8963 ak8963 = new Ak8963(I2cDevice.Create(mpui2CConnectionSettingmpus));
Debug.WriteLine(
    "Magnetometer calibration is taking couple of seconds, move your sensor in all possible directions! Make sure you don't have a magnet or phone close by.");
Vector3 mag = ak8963.CalibrateMagnetometer();
Debug.WriteLine($"Bias:");
Debug.WriteLine($"Mag X = {mag.X}");
Debug.WriteLine($"Mag Y = {mag.Y}");
Debug.WriteLine($"Mag Z = {mag.Z}");
Debug.WriteLine("Press a key to continue");
Thread.Sleep(1000);

while (true)
{
    Vector3 magne = ak8963.ReadMagnetometer(true, TimeSpan.FromMilliseconds(11));
    Debug.WriteLine($"Mag X = {magne.X,15}");
    Debug.WriteLine($"Mag Y = {magne.Y,15}");
    Debug.WriteLine($"Mag Z = {magne.Z,15}");
    Thread.Sleep(200);
}
