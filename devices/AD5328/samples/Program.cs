// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Spi;
using System.Threading;
using Iot.Device.DAC;
using UnitsNet;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the SPI GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.SPI1_MOSI);
//Configuration.SetPinFunction(22, DeviceFunction.SPI1_MISO);
//Configuration.SetPinFunction(22, DeviceFunction.SPI1_CLOCK);
// Make sure as well you are using the right chip select
var spisettings = new SpiConnectionSettings(1, 42)
{
    Mode = SpiMode.Mode2
};

var spidev = SpiDevice.Create(spisettings);
using var dac = new AD5328(spidev, ElectricPotential.FromVolts(2.5), ElectricPotential.FromVolts(2.5));
Thread.Sleep(1000);
dac.SetVoltage(0, ElectricPotential.FromVolts(1));
