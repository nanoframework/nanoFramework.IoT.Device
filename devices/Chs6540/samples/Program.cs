// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Chs6540;
using nanoFramework.Hardware.Esp32;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new(1, Chs6540.DefaultI2cAddress);
using I2cDevice device = I2cDevice.Create(settings);
using GpioController gpio = new();

using Chs6540 sensor = new(device);
sensor.SetInterruptMode(true);

gpio.OpenPin(39, PinMode.Input);
// This will enable an event on GPIO39 on falling edge when the screen if touched
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
        Debug.WriteLine($"ID: {point.TouchId}, X: {point.X}, Y: {point.Y}, Weight: {point.Weigth}, Misc: {point.Miscelaneous}, Event: {point.Event}");
    }
    else if (points == 2)
    {
        var dp = sensor.GetDoublePoints();
        Debug.WriteLine($"ID: {dp.Point1.TouchId}, X: {dp.Point1.X}, Y: {dp.Point1.Y}, Weight: {dp.Point1.Weigth}, Misc: {dp.Point1.Miscelaneous}, Event: {dp.Point1.Event}");
        Debug.WriteLine($"ID: {dp.Point2.TouchId}, X: {dp.Point2.X}, Y: {dp.Point2.Y}, Weight: {dp.Point2.Weigth}, Misc: {dp.Point2.Miscelaneous}, Event: {dp.Point1.Event}");
    }
}