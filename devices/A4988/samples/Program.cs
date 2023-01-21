// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.A4988;
using System;

// Pinout for MCU please adapt depending on your MCU
// Any regular GPIO will work
const byte stepPin = 10;
const byte dirPin = 11;
const Microsteps microsteps = Microsteps.FullStep;
const ushort fullStepsPerRotation = 200;
TimeSpan sleepTime = TimeSpan.Zero;
using (var motor = new A4988(stepPin, dirPin, microsteps, fullStepsPerRotation, sleepTime))
{
    var direction = true;
    while (true)
    {
        var rotationDegree = (direction ? 1 : -1) * 360;
        motor.Rotate(UnitsNet.Angle.FromDegrees(rotationDegree));
        direction = !direction;
        System.Threading.Thread.Sleep(1000);
    }
}