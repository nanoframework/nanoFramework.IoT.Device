// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Device.Gpio;
using System.Threading;
using Iot.Device.LiquidLevel;

using LiquidLevelSwitch sensor = new(23, PinValue.Low);
while (true)
{
    // read liquid level switch
    Debug.WriteLine($"Detected: {sensor.IsLiquidPresent()}");
    Debug.WriteLine("");

    Thread.Sleep(1000);
}
