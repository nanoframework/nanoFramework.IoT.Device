// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Adc;
using nanoFramework.Hardware.Esp32;
using UnitsNet;

Configuration.SetPinFunction(Gpio.IO21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(Gpio.IO22, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new I2cConnectionSettings(1, Ina236.DefaultI2cAddress);
I2cDevice i2cDevice = new I2cDevice(settings);

using (Ina236 device = new Ina236(
    i2cDevice,
    ElectricResistance.FromMilliohms(8),
    ElectricCurrent.FromAmperes(10)))
{
    Debug.WriteLine($"Operating mode: {device.OperatingMode}");
    Debug.WriteLine($"Samples averaged: {device.AverageOverNoSamples}");
    Debug.WriteLine($"Bus conversion time: {device.BusConversionTime} us");
    Debug.WriteLine($"Shunt conversion time: {device.ShuntConversionTime} us");

    while (true)
    {
        Debug.WriteLine($"Bus: {device.ReadBusVoltage().Volts} V, Shunt: {device.ReadShuntVoltage().Millivolts} mV, Current: {device.ReadCurrent().Amperes} A, Power: {device.ReadPower().Watts} W");
        Thread.Sleep(1000);
    }
}