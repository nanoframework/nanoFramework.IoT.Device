// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Vl53L0X;
using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

Debug.WriteLine("Hello VL53L0X!");
using Vl53L0X vL53L0X = new(I2cDevice.Create(new I2cConnectionSettings(1, Vl53L0X.DefaultI2cAddress)));
Debug.WriteLine($"Rev: {vL53L0X.Information.Revision}, Prod: {vL53L0X.Information.ProductId}, Mod: {vL53L0X.Information.ModuleId}");
Debug.WriteLine($"Offset in µm: {vL53L0X.Information.OffsetMicrometers}, Signal rate fixed 400 µm: {vL53L0X.Information.SignalRateMeasuementFixed400Micrometers}");
vL53L0X.MeasurementMode = MeasurementMode.Continuous;
while (true)
{
    try
    {
        var dist = vL53L0X.Distance;
        if (dist != (ushort)OperationRange.OutOfRange)
        {
            Debug.WriteLine($"Distance: {dist}");
        }
        else
        {
            Debug.WriteLine("Invalid data");
        }
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"Exception: {ex.Message}");
    }

    Thread.Sleep(500);
}

/*
// Code snippet to change the address of a sensor
byte newAddress = 0x28; // You can change for any valid supported address
I2cDevice connectionToCurrentAddress = I2cDevice.Create(new I2cConnectionSettings(1, Vl53L0X.DefaultI2cAddress));
Vl53L0X.ChangeI2cAddress(connectionToCurrentAddress, newAddress);
I2cDevice connectionToChangedAddress = I2cDevice.Create(new I2cConnectionSettings(1, newAddress));
using Vl53L0X vlWithChangedAddress = new(connectionToChangedAddress);
// Do what you'd like to do here
// Soft reset the device to setup the default address again
vlWithChangedAddress.Reset();
*/
