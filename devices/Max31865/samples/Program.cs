// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Spi;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Max31865;
using UnitsNet;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the SPI GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.SPI1_MOSI);
//Configuration.SetPinFunction(22, DeviceFunction.SPI1_MISO);
//Configuration.SetPinFunction(22, DeviceFunction.SPI1_CLOCK);
// Make sure as well you are using the right chip select
SpiConnectionSettings settings = new(1, 42)
{
    ClockFrequency = Max31865.SpiClockFrequency,
    Mode = Max31865.SpiMode1,
    DataFlow = Max31865.SpiDataFlow
};

using SpiDevice device = SpiDevice.Create(settings);
using Max31865 sensor = new(device, PlatinumResistanceThermometerType.Pt1000, ResistanceTemperatureDetectorWires.ThreeWire, ElectricResistance.FromOhms(4300));

while (true)
{
    Debug.WriteLine($"Temperature: {sensor.Temperature.DegreesCelsius} ℃");

    // wait for 2000ms
    Thread.Sleep(2000);
}
