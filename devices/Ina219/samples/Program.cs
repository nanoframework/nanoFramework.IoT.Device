// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Device.I2c;
using Iot.Device;
using Iot.Device.Adc;
using System.Diagnostics;
using nanoFramework.Hardware.Esp32;

const byte Ina219_I2cAddress = 0x40;
const byte Ina219_I2cBus = 0x1;


// Must specify pin functions on ESP32, not needed for most other boards
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

// create an INA219 device on I2C bus 1 addressing channel 64
using Ina219 device = new(new I2cConnectionSettings(Ina219_I2cBus, Ina219_I2cAddress));
// reset the device
device.Reset();

// set up the bus and shunt voltage ranges and the calibration. Other values left at default.
device.BusVoltageRange = Ina219BusVoltageRange.Range16v;
device.PgaSensitivity = Ina219PgaSensitivity.PlusOrMinus40mv;
device.SetCalibration(33574, 12.2e-6f);

while (true)
{
    // write out the current values from the INA219 device.
    Debug.WriteLine($"Bus Voltage {device.ReadBusVoltage().Volts} Shunt Voltage {device.ReadShuntVoltage().Millivolts}mV Current {device.ReadCurrent().Value} Power {device.ReadPower().Watts} [OVF = {device.MathOverflowFlag}]");
    Thread.Sleep(1000);
}
