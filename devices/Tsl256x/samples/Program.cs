// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Tsl256x;
using nanoFramework.Hardware.Esp32;

// Must specify pin functions on ESP32, not needed for most other boards
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

const int PinInterrupt = 4;

Debug.WriteLine("Hello TSL256x");
I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(1, Tsl256x.DefaultI2cAddress));
Tsl256x tsl256X = new(i2cDevice, PackageType.Other);
var ver = tsl256X.Version;
string msg = (ver.Major & 0x01) == 0x01 ? $"This is a TSL2561, Version {ver}" : $"This is a TSL2560, Version {ver}";
Debug.WriteLine(msg);

tsl256X.IntegrationTime = IntegrationTime.Integration402Milliseconds;
tsl256X.Gain = Gain.Normal;

Debug.WriteLine("This will get the illuminance with the standard period of 402 ms integration and normal gain");

for (var j = 0; j < 20; j++)
{
    var lux = tsl256X.MeasureAndGetIlluminance();
    Debug.WriteLine($"Illuminance is {lux.Lux} Lux");
    tsl256X.GetRawChannels(out ushort channel0, out ushort channel1);
    Debug.WriteLine($"Raw data channel 0: {channel0}, channel 1: {channel1}");
    Thread.Sleep(500);
}

Debug.WriteLine($"Try changing the gain and read it from {tsl256X.Gain} to {Gain.High}");
tsl256X.Gain = Gain.High;
Debug.WriteLine($"New gain {tsl256X.Gain}");

Debug.WriteLine($"Try changing the integration time and read it from {tsl256X.IntegrationTime} to {IntegrationTime.Integration13_7Milliseconds}");
tsl256X.IntegrationTime = IntegrationTime.Integration13_7Milliseconds;
Debug.WriteLine($"New integration time {tsl256X.IntegrationTime}");

Debug.WriteLine("Set power on and check it");
tsl256X.Enabled = true;
Debug.WriteLine($"Power should be true: {tsl256X.Enabled}");
tsl256X.Enabled = false;
Debug.WriteLine($"Power should be false: {tsl256X.Enabled}");
tsl256X.Enabled = true;

GpioController controller = new();
controller.OpenPin(PinInterrupt, PinMode.Input);
Debug.WriteLine($"Pin status: {controller.Read(PinInterrupt)}");
Debug.WriteLine("Set interruption to test. Read the interrupt pin");
tsl256X.InterruptControl = InterruptControl.TestMode;
tsl256X.Enabled = true;
while (controller.Read(PinInterrupt) == PinValue.High)
{
    Thread.Sleep(1);
}

tsl256X.Enabled = false;
Debug.WriteLine($"Interrupt detected, read the value to clear the interrupt");
tsl256X.GetRawChannels(out ushort ch0, out ushort ch1);
Debug.WriteLine($"Raw data channel 0 {ch0}, channel 1 {ch1}");
if (controller.Read(PinInterrupt) == PinValue.Low)
{
    Debug.WriteLine("Interrupt properly cleaned");
}
else
{
    Debug.WriteLine("Interrupt not cleaned");
}

// Adjust those values with a previous measurement to understand the conditions, find a level where then you can
// hide the sensor with your arm and make it going under the minimum level or vice versa with a lamp
tsl256X.SetThreshold(0x0000, 0x00FF);
tsl256X.InterruptPersistence = InterruptPersistence.OutOfRange06IntegrationTimePeriods;
tsl256X.InterruptControl = InterruptControl.LevelInterrupt;
tsl256X.Enabled = true;
while (controller.Read(PinInterrupt) == PinValue.High)
{
    Thread.Sleep(1);
}

Debug.WriteLine($"Interrupt detected, read the value to clear the interrupt");
tsl256X.Enabled = false;
tsl256X.GetRawChannels(out ch0, out ch1);
Debug.WriteLine($"Raw data channel 0 {ch0}, channel 1 {ch1}");

Debug.WriteLine("This will use a manual integration for 5 seconds");
tsl256X.StartManualIntegration();
Thread.Sleep(5000);
tsl256X.StopManualIntegration();
tsl256X.GetRawChannels(out ch0, out ch1);
Debug.WriteLine($"Raw data channel 0 {ch0}, channel 1 {ch1}");
Debug.WriteLine($"Integration time should then be {IntegrationTime.Manual} = {tsl256X.IntegrationTime}");
