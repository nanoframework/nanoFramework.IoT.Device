// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Seesaw;

const byte AdafruitSeesawBreakoutI2cAddress = 0x49;
const byte AdafruitSeesawBreakoutI2cBus = 0x1;

using I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(AdafruitSeesawBreakoutI2cBus, AdafruitSeesawBreakoutI2cAddress));
using Seesaw ssDevice = new(i2cDevice);
Console.WriteLine("");
Console.WriteLine($"Seesaw Version: {ssDevice.Version}");
Console.WriteLine("");

Console.WriteLine(GetModuleAvailability(ssDevice, Seesaw.SeesawModule.Status));
Console.WriteLine(GetModuleAvailability(ssDevice, Seesaw.SeesawModule.Gpio));
Console.WriteLine(GetModuleAvailability(ssDevice, Seesaw.SeesawModule.Sercom0));
Console.WriteLine(GetModuleAvailability(ssDevice, Seesaw.SeesawModule.Timer));
Console.WriteLine(GetModuleAvailability(ssDevice, Seesaw.SeesawModule.Adc));
Console.WriteLine(GetModuleAvailability(ssDevice, Seesaw.SeesawModule.Dac));
Console.WriteLine(GetModuleAvailability(ssDevice, Seesaw.SeesawModule.Interrupt));
Console.WriteLine(GetModuleAvailability(ssDevice, Seesaw.SeesawModule.Dap));
Console.WriteLine(GetModuleAvailability(ssDevice, Seesaw.SeesawModule.Eeprom));
Console.WriteLine(GetModuleAvailability(ssDevice, Seesaw.SeesawModule.Neopixel));
Console.WriteLine(GetModuleAvailability(ssDevice, Seesaw.SeesawModule.Touch));
Console.WriteLine(GetModuleAvailability(ssDevice, Seesaw.SeesawModule.Keypad));
Console.WriteLine(GetModuleAvailability(ssDevice, Seesaw.SeesawModule.Encoder));

Console.WriteLine("");

static string GetModuleAvailability(Seesaw ssDevice, Seesaw.SeesawModule module)
{
    var moduleAvailable = ssDevice.HasModule(module) ? "available" : "not-available";
    return $"Module: {module.ToString()} - {moduleAvailable}";
}
