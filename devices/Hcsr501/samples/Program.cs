// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using System.Device.Gpio;
using System.Diagnostics;
using Iot.Device.Hcsr501;

const int Hcsr501Pin = 17;
const int LedPin = 27;

using GpioController ledController = new();
ledController.OpenPin(LedPin, PinMode.Output);

using Hcsr501 sensor = new(Hcsr501Pin);
while (true)
{
    // adjusting the detection distance and time by rotating the potentiometer on the sensor
    if (sensor.IsMotionDetected)
    {
        // turn the led on when the sensor detected infrared heat
        ledController.Write(LedPin, PinValue.High);
        Debug.WriteLine("Detected! Turn the LED on.");
    }
    else
    {
        // turn the led off when the sensor undetected infrared heat
        ledController.Write(LedPin, PinValue.Low);
        Debug.WriteLine("Undetected! Turn the LED off.");
    }

    Thread.Sleep(1000);
}
