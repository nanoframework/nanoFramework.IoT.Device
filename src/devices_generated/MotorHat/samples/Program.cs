// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Threading;
using Iot.Device.MotorHat;
using Iot.Device.DCMotor;

const double Period = 10.0;
Stopwatch sw = Stopwatch.StartNew();

// Use the following code to generate an I2C address different from the default
// var busId = 1;
// var selectedI2cAddress = 0b000000;     // A5 A4 A3 A2 A1 A0
// var deviceAddress = MotorHat.I2cAddressBase + selectedI2cAddress;
// I2cConnectionSettings settings = new(busId, deviceAddress);
using MotorHat motorHat = new();
using DCMotor motor = motorHat.CreateDCMotor(1);

bool done = false;
Console.CancelKeyPress += (o, e) =>
{
    done = true;
    e.Cancel = true;
};

string? lastSpeedDisp = null;
while (!done)
{
    double time = sw.ElapsedMilliseconds / 1000.0;

    // Note: range is from -1 .. 1 (for 1 pin setup 0 .. 1)
    motor.Speed = Math.Sin(2.0 * Math.PI * time / Period);
    string disp = $"Speed = {motor.Speed:0.00}";
    if (disp != lastSpeedDisp)
    {
        lastSpeedDisp = disp;
        Debug.WriteLine(disp);
    }

    Thread.Sleep(1);
}
