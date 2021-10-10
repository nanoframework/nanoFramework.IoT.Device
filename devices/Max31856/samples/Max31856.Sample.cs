// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Spi;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Max31856;
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
    ClockFrequency = Max31856.SpiClockFrequency,
    Mode = Max31856.SpiModeSetup,
    DataFlow = Max31856.SpiDataFlow
};

using SpiDevice device = SpiDevice.Create(settings);
using Max31856 sensor = new(device, ThermocoupleType.K);
while (true)
{
    Temperature tempThermocouple = sensor.GetTemperature();
    Debug.WriteLine($"Temperature Thermocouple: {tempThermocouple.DegreesFahrenheit} F");
    Temperature tempColdJunction = sensor.GetColdJunctionTemperature();
    Debug.WriteLine($"Temperature Cold Junction: {tempColdJunction.DegreesFahrenheit} F");
    Thread.Sleep(2000);
}
