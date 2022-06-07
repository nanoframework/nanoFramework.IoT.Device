// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Chsc6540;
using nanoFramework.Hardware.Esp32;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new(1, Chsc6540.DefaultI2cAddress);
using I2cDevice device = I2cDevice.Create(settings);
using GpioController gpio = new();

using Chsc6540 sensor = new(device);
sensor.SetInterruptMode(true);

// This will enable an event on GPIO39 on falling edge when the screen if touched
gpio.RegisterCallbackForPinValueChangedEvent(39, PinEventTypes.Falling, TouchInterrupCallback);

while (true)
{
    Thread.Sleep(20);
}

void TouchInterrupCallback(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
{
    Debug.WriteLine("Touch interrupt");

    var points = sensor.GetDoublePoints();

    if (points.Point1.Event > Event.NoEvent)
    {
        Debug.WriteLine($"P1 X: {points.Point1.X}, Y: {points.Point1.Y}, Event: {points.Point1.Event}");
    }
    else if (points.Point2.Event > Event.NoEvent)
    {
        Debug.WriteLine($"P2 X: {points.Point2.X}, Y: {points.Point2.Y}, Event: {points.Point2.Event}");
    }
}