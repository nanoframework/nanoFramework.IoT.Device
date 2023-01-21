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
// For more details on the connections, please refer to the following
// https://github.com/nanoframework/nanoFramework.IoT.Device/tree/develop/devices/Hx711#signals-and-connections
// MOSI: connects to WEIGHT Grove pin 2 (PD_SCK)
// MISO: connects to WEIGHT Grove pin 1 (DOUT)
// CLOCK: connect to any free port as it's not used at all
Configuration.SetPinFunction(21, DeviceFunction.SPI1_MOSI);
Configuration.SetPinFunction(22, DeviceFunction.SPI1_MISO);
Configuration.SetPinFunction(23, DeviceFunction.SPI1_CLOCK);

// setup SPI connection settings
// the clock value was adjusted in order to get the typical duration expected by the PD_SCK ~1us
var spisettings = new SpiConnectionSettings(1)
{
    ClockFrequency = Scale.DefaultClockFrequency
};

// create SPI device
var spidev = SpiDevice.Create(spisettings);

// create scale
var scale = new Scale(spidev);

// power up WEIGHT module
// select channel A with gain 64
scale.PowerUp(GainLevel.GainA64);
// select channel A with gain 128 for 2 times better scale resolution but more noise too
//scale.PowerUp(GainLevel.GainA64);

// set avaraging to 10 samples for tare
scale.SampleAveraging = 10;
// set scale tare to get accurate readings
scale.Tare();

// set avaraging to 3 samples for loop sampling
scale.SampleAveraging = 3;

//example gramm value to convert measurments to actual weight
double gramm_unit = 220.23;
// loop forever outputting the current reading
while (true)
{
    var reading = scale.Read();

    Console.WriteLine($"Read value: {reading} Weight: {reading / gramm_unit} gramm");

    Thread.Sleep(2_000);
}
