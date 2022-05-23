// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Hx711;
using nanoFramework.Hardware.Esp32;
using System;
using System.Device.Spi;
using System.Threading;

///////////////////////////////////////////////////////////////////////
// When connecting to an ESP32 device, need to configure the SPI GPIOs
// The following mapping is used in order to connect the WEIGTH module
// to a M5Core device using the Grove port A
// MOSI: connects to Grove pin 1
// MISO: connects to Grove pin 2
// CLOCK: connect to any free port as it's not used at all
Configuration.SetPinFunction(21, DeviceFunction.SPI1_MOSI);
Configuration.SetPinFunction(22, DeviceFunction.SPI1_MISO);
Configuration.SetPinFunction(23, DeviceFunction.SPI1_CLOCK);

// setup SPI connection settings
// the clock value was adjusted in order to get the typical duration expected by the PD_SCK ~1us
var spisettings = new SpiConnectionSettings(2, 19)
{
    ClockFrequency = Scale.DefaultClockFrequency
};

// create SPI device
var spidev = SpiDevice.Create(spisettings);

// create scale
var scale = new Scale(spidev);

// power up WEIGHT module
scale.PowerUp();

// set scale tare to get accurate readings
scale.Tare();

// loop forever outputting the current reading
while (true)
{
    var reading = scale.Read();

    Console.WriteLine($"Weight: {reading}");

    Thread.Sleep(2_000);
}
