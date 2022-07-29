// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Mcp23xxx;
using nanoFramework.Hardware.Esp32;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

// redefine I2C pins to match your ESP32 DevKit
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

// set I2C bus ID: 1
// 0x20 is the device address
I2cConnectionSettings settings = new I2cConnectionSettings(1, 0x20, I2cBusSpeed.StandardMode);
I2cDevice i2cDevice = I2cDevice.Create(settings);
Mcp23017 mcp23017 = new Mcp23017(i2cDevice);

// set port A0 to output mode
var portAIODirRegister = mcp23017.ReadByte(Register.IODIR, Port.PortA);
portAIODirRegister &= 0b1111_1110;
mcp23017.WriteByte(Register.IODIR, portAIODirRegister, Port.PortA);
Debug.WriteLine($"port A IO direction register: 0x{mcp23017.ReadByte(Register.IODIR, Port.PortA)}");

bool portA0 = true;

while (true)
{
	portA0 = !portA0;
	var portGPIORegisterPortA = mcp23017.ReadByte(Register.GPIO, Port.PortA);
    if (portA0)
    {
        portGPIORegisterPortA |= 0b0000_0001;
    }
    else
    {
        portGPIORegisterPortA &= 0b1111_1110;
    }	
    mcp23017.WriteByte(Register.GPIO, portGPIORegisterPortA, Port.PortA);

    Thread.Sleep(1000);
}

