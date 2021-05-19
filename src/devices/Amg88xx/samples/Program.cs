// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Amg88xx;
using UnitsNet;

// Setup I2C bus for communicating with the AMG8833 sensor.
// Note: if you're using a breakout board check which address is configured by the logic level
// of the sensor's AD_SELECT pin.
const int I2cBus = 1;
I2cConnectionSettings i2cSettings = new I2cConnectionSettings(I2cBus, Amg88xx.AlternativeI2cAddress);
I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);

// Setup GPIO controller for receiving interrupts from the sensor's INT pin (pin 5)
// and for driving an LED (pin 6) as an interrupt indicator.
GpioController ioController = new GpioController();
ioController.OpenPin(5, PinMode.Input);
ioController.OpenPin(6, PinMode.Output);
ioController.Write(6, PinValue.Low);

// Hook up interrupt handler for the falling edge. This gets invoked
// if any pixel exceeds the configured upper or lower interrupt level.
// At the same time the interrupt flag is set in the status register.
// The INT signal will stay low as long as any pixel is exceeding the threshold
// or the reading is within the hystersis span. In the latter case you can clear the interrupt
// by doing a flags reset.
ioController.RegisterCallbackForPinValueChangedEvent(5, PinEventTypes.Falling, (s, e) =>
{
    ioController.Write(6, PinValue.High);
});

// Same as above but for the rising edge, which happens if the interrupt condition is
// resolved.
ioController.RegisterCallbackForPinValueChangedEvent(5, PinEventTypes.Rising, (s, e) =>
{
    ioController.Write(6, PinValue.Low);
});

Amg88xx amg88xx = new Amg88xx(i2cDevice);

// factory defaults
amg88xx.Reset();

amg88xx.OperatingMode = OperatingMode.Normal;
Debug.WriteLine($"Operating mode: {amg88xx.OperatingMode}");

// Switch moving average mode on.
// amg88xx.UseMovingAverageMode = true;
// Note: reading the average mode state doesn't seems to work with current revisions
//       of the sensor, even though the reference specification defines the register
//      as R/W type.
Debug.WriteLine($"Average mode: {amg88xx.UseMovingAverageMode}");

// Set frame rate to 1 fps
// amg88xx.FrameRate = FrameRate.Rate1FramePerSecond;
Debug.WriteLine($"Frame rate: {amg88xx.FrameRate}");

// set interrupt mode and levels
amg88xx.InterruptMode = InterruptMode.Absolute;
// enable the interrupt output pin (INT)
amg88xx.InterruptPinEnabled = true;
// set the hysteresis span to 4°C
amg88xx.InterruptHysteresis = Temperature.FromDegreesCelsius(4);
// Set the lower level to 10°C. The interrupt is raised when the temperature of any pixel goes below 10°C.
// Due to the hysteresis level the interrupt is not cleared before all pixels are above 14°C.
amg88xx.InterruptLowerLevel = Temperature.FromDegreesCelsius(10);
// Set the upper level to 28°C. The interrupt is raised when the temperature of any pixel goes over 28°C.
// Due to the hysteresis level the interrupt is not cleared before all pixels are below 24°C.
amg88xx.InterruptUpperLevel = Temperature.FromDegreesCelsius(28);

Debug.WriteLine($"Interrupt mode: {amg88xx.InterruptMode}");
Debug.WriteLine($"Lower interrupt temperature level: {amg88xx.InterruptLowerLevel.DegreesCelsius:F1}°C");
Debug.WriteLine($"Upper interrupt temperature level: {amg88xx.InterruptUpperLevel.DegreesCelsius:F1}°C");
Debug.WriteLine($"Hysteresis level: {amg88xx.InterruptHysteresis.DegreesCelsius:F1}°C");

while (true)
{
    Debug.WriteLine($"Thermistor: {amg88xx.SensorTemperature}");
    Debug.WriteLine($"Interrupt occurred: {amg88xx.HasInterrupt()}");
    Debug.WriteLine($"Temperature overflow: {amg88xx.HasTemperatureOverflow()}");
    Debug.WriteLine($"Thermistor overflow: {amg88xx.HasThermistorOverflow()}");

    // Optionally check whether the thermistor temperature or any pixel temperature
    // exceeds maximum levels.
    // Debug.WriteLine($"Temperature overrun: {amg88xx.HasTemperatureOverflow()}");
    // Debug.WriteLine($"Thermistor overrun: {amg88xx.HasThermistorOverflow()}");

    // Get the current thermal image and the interrupt flags.
    // Note: this isn't and can't be synchronized with the internal sampling
    // of the sensor.
    amg88xx.ReadImage();

    var intFlags = amg88xx.GetInterruptFlagTable();

    // Display the pixel temperature readings and an interrupt indicator in the console.
    for (int r = 0; r < Amg88xx.Height; r++)
    {
        for (int c = 0; c < Amg88xx.Width; c++)
        {
            Point pt = new Point(c, r);
            Debug.Write($"{(intFlags[c][r] ? '*' : ' ')}  {amg88xx[pt].DegreesCelsius,6:F2}");
        }

        Debug.WriteLine("\n------------------------------------------------------------------------");
    }

    // Example how to get all 64 pixels as raw readings in 12-bit two's complement format.
    /*
    for (int n = 0; n < Amg88xx.PixelCount; n += 8)
    {
        Debug.WriteLine($"{amg88xx[n]} {amg88xx[n + 1]} {amg88xx[n + 2]} {amg88xx[n + 3]} {amg88xx[n + 4]} {amg88xx[n + 5]} {amg88xx[n + 6]} {amg88xx[n + 7]} ");
    }
    */

    Debug.WriteLine("");

    // Resetting flags manually can be used to clear all interrrupt flags and to release the INT pin
    // while all pixels are within the range of the lower and upper interrupt levels but one or more
    // pixel is still within the hysteresis range.
    // amg88xx.FlagReset();
    Thread.Sleep(1000);
}
