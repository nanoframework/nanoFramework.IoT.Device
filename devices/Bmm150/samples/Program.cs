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

//Debug.WriteLine(
//    "Magnetometer calibration is taking couple of seconds, move your sensor in all possible directions! Make sure you don't have a magnet or phone close by.");
//Vector3 mag = bmm150.CalibrateMagnetometer();
//Debug.WriteLine($"Bias:");
//Debug.WriteLine($"Mag X = {mag.X}");
//Debug.WriteLine($"Mag Y = {mag.Y}");
//Debug.WriteLine($"Mag Z = {mag.Z}");
//Debug.WriteLine("Press a key to continue");
//Thread.Sleep(1000);

while (true)
{
    Vector3 magne = bmm150.ReadMagnetometer(true, TimeSpan.FromMilliseconds(11));
    Debug.WriteLine($"Mag X = {magne.X,15}, Y = {magne.Y,15}, Z = {magne.Z,15}");
    
    Thread.Sleep(200);
}
