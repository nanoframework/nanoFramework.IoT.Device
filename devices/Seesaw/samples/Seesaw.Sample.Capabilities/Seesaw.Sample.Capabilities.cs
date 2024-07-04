// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Seesaw;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus.
// using nanoFramework.Hardware.Esp32;
// Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
// Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

const byte AdafruitSeesawBreakoutI2cAddress = 0x49;
const byte AdafruitSeesawBreakoutI2cBus = 0x1;

using I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(AdafruitSeesawBreakoutI2cBus, AdafruitSeesawBreakoutI2cAddress));
using Seesaw ssDevice = new(i2cDevice);
Console.WriteLine("");
Console.WriteLine($"Seesaw Version: {ssDevice.Version}");
Console.WriteLine("");

Console.WriteLine(GetModuleAvailability(ssDevice, SeesawModule.Status));
Console.WriteLine(GetModuleAvailability(ssDevice, SeesawModule.Gpio));
Console.WriteLine(GetModuleAvailability(ssDevice, SeesawModule.Sercom0));
Console.WriteLine(GetModuleAvailability(ssDevice, SeesawModule.Timer));
Console.WriteLine(GetModuleAvailability(ssDevice, SeesawModule.Adc));
Console.WriteLine(GetModuleAvailability(ssDevice, SeesawModule.Dac));
Console.WriteLine(GetModuleAvailability(ssDevice, SeesawModule.Interrupt));
Console.WriteLine(GetModuleAvailability(ssDevice, SeesawModule.Dap));
Console.WriteLine(GetModuleAvailability(ssDevice, SeesawModule.Eeprom));
Console.WriteLine(GetModuleAvailability(ssDevice, SeesawModule.Neopixel));
Console.WriteLine(GetModuleAvailability(ssDevice, SeesawModule.Touch));
Console.WriteLine(GetModuleAvailability(ssDevice, SeesawModule.Keypad));
Console.WriteLine(GetModuleAvailability(ssDevice, SeesawModule.Encoder));

Console.WriteLine("");

static string GetModuleAvailability(Seesaw ssDevice, SeesawModule module)
{
    var moduleAvailable = ssDevice.HasModule(module) ? "available" : "not-available";
    return $"Module: {module.ToString()} - {moduleAvailable}";
}
