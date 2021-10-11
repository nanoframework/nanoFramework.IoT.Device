// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Spi;
using System.Drawing;
using System.Threading;
using Iot.Device.Apa102;

var random = new Random();

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the SPI GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.SPI1_MOSI);
//Configuration.SetPinFunction(22, DeviceFunction.SPI1_MISO);
//Configuration.SetPinFunction(22, DeviceFunction.SPI1_CLOCK);
// Make sure as well you are using the right chip select
using SpiDevice spiDevice = SpiDevice.Create(new SpiConnectionSettings(1, 42)
{
    ClockFrequency = 20_000_000,
    DataFlow = DataFlow.MsbFirst,
    Mode = SpiMode.Mode0 // ensure data is ready at clock rising edge
});
using Apa102 apa102 = new Apa102(spiDevice, 16);

while (true)
{
    var pixels = apa102.Pixels;
    for (var i = 0; i < apa102.Pixels.Length; i++)
    {
        pixels[i] = Color.FromArgb(255, random.Next(256), random.Next(256), random.Next(256));
    }

    apa102.Flush();
    Thread.Sleep(1000);
}
