// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Mcp960x;
using nanoFramework.Hardware.Esp32;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

Debug.WriteLine("Write I2C MCP960X - ADR 0x67 - Read ambient and hot junction temperature every 1 sec");

// redefine I2C pins to match your ESP32 DevKit
// Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
// Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

// set I2C bus ID: 1
// 0x67 is the device address
I2cConnectionSettings settings = new I2cConnectionSettings(1, 0x67);
I2cDevice i2cDevice = I2cDevice.Create(settings);
Mcp960x mcp960x = new Mcp960x(i2cDevice, coldJunctionResolutionType: ColdJunctionResolutionType.N_0_25);

DeviceIDType deviceIDType;
byte major;
byte minor;
mcp960x.ReadDeviceID(out deviceIDType, out major, out minor);
Debug.WriteLine($"device id: {(byte)deviceIDType} - major: {major} - minor: {minor}");

while (true)
{
    Debug.WriteLine($"ambient temperture: {mcp960x.GetColdJunctionTemperature()}");
    Debug.WriteLine($"hot junction temperture: {mcp960x.GetTemperature()}");

    Thread.Sleep(1000);
}

