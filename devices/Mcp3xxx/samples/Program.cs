// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Spi;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Adc;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the SPI GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.SPI1_MOSI);
//Configuration.SetPinFunction(22, DeviceFunction.SPI1_MISO);
//Configuration.SetPinFunction(22, DeviceFunction.SPI1_CLOCK);
// Make sure as well you are using the right chip select
var hardwareSpiSettings = new SpiConnectionSettings(1, 42)
{
    ClockFrequency = 1000000
};

using SpiDevice spi = new SpiDevice(hardwareSpiSettings);
// For hardware implementation replace it with following
// using (SpiDevice spi = SpiDevice.Create(hardwareSpiSettings))
using Mcp3008 mcp = new Mcp3008(spi);
while (true)
{
    double value = mcp.Read(0);
    value = value / 10.24;
    value = Math.Round(value);
    Debug.WriteLine($"{value}%");
    Thread.Sleep(500);
}
