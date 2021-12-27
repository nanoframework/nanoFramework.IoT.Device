// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Ft6xx6x;
using nanoFramework.Hardware.Esp32;
using nanoFramework.M5Stack;

M5Core2.InitializeScreen();

I2cConnectionSettings settings = new(1, Ft6xx6x.DefaultI2cAddress);
using I2cDevice device = I2cDevice.Create(settings);
using GpioController gpio = new();

using Ft6xx6x sensor = new(device);
//sensor.ChargerOn = true;
var ver = sensor.GetVersion();
Debug.WriteLine($"version: {ver}");
sensor.SetInterruptMode(false);
Debug.WriteLine($"Period active: {sensor.PeriodActive}");
Debug.WriteLine($"Period active in monitor mode: {sensor.MonitorModePeriodActive}");
Debug.WriteLine($"Time to enter monitor: {sensor.MonitorModeDelaySeconds} seconds");
Debug.WriteLine($"Monitor mode: {sensor.MonitorModeEnabled}");
Debug.WriteLine($"Proximity sensing: {sensor.ProximitySensingEnabled}");

gpio.OpenPin(39, PinMode.Input);
gpio.RegisterCallbackForPinValueChangedEvent(39, PinEventTypes.Falling, TouchInterrupCallback);

while (true)
{
    Thread.Sleep(20);
}

void TouchInterrupCallback(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
{
    Debug.WriteLine("Touch interrupt");
    var points = sensor.GetNumberPoints();
    if (points == 1)
    {
        var point = sensor.GetPoint(true);
        Debug.WriteLine($"ID: {point.TouchId}, X: {point.X}, Y: {point.Y}, Weight: {point.Weigth}, Misc: {point.Miscelaneous}");
    }
    else if (points == 2)
    {
        var dp = sensor.GetDoublePoints();
        Debug.WriteLine($"ID: {dp.Point1.TouchId}, X: {dp.Point1.X}, Y: {dp.Point1.Y}, Weight: {dp.Point1.Weigth}, Misc: {dp.Point1.Miscelaneous}");
        Debug.WriteLine($"ID: {dp.Point2.TouchId}, X: {dp.Point2.X}, Y: {dp.Point2.Y}, Weight: {dp.Point2.Weigth}, Misc: {dp.Point2.Miscelaneous}");
    }
}