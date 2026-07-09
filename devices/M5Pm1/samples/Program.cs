// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.M5Pm1;
using nanoFramework.Hardware.Esp32;
using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

// This sample targets the M5Stack M5StickS3 power-management IC (M5PM1) on the internal I2C bus.
//
// M5StickS3 wiring (see the M5Stack M5GFX / M5Unified sources):
//   I2C control : SCL = GPIO48, SDA = GPIO47 (internal system bus, address 0x6E)

// Setup the M5StickS3 internal I2C bus (SDA = GPIO47, SCL = GPIO48).
Configuration.SetPinFunction(47, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(48, DeviceFunction.I2C1_CLOCK);

// The M5PM1 is a 100 kHz (standard-mode) device; the ESP32 default is 400 kHz, which the PMIC NAKs.
I2cConnectionSettings settings = new I2cConnectionSettings(1, M5Pm1.I2cDefaultAddress, I2cBusSpeed.StandardMode);
I2cDevice i2cDevice = new I2cDevice(settings);

M5Pm1 power = new M5Pm1(i2cDevice);

// The constructor wakes the PMIC and applies the M5Stack reliability init (the M5PM1 sleeps on an idle
// I2C bus). Optionally confirm the device ID.
int deviceId = power.GetDeviceId();
Debug.WriteLine($"M5PM1 device ID: 0x{deviceId:X4} (expected 0x{M5Pm1.DeviceId:X4}).");

// Enable battery charging and the external 5V output.
power.BatteryChargeEnabled = true;
power.ExternalOutputEnabled = true;

// GPIO example: drive GPIO2 high as a push-pull output (on the M5StickS3 this is the L3B / LCD power
// gate), and read GPIO0 as an input (the M5StickS3 charge-status line).
power.SetGpioFunction(Pin.Gpio2, GpioFunction.Gpio);
power.SetGpioDrive(Pin.Gpio2, GpioDrive.PushPull);
power.SetGpioMode(Pin.Gpio2, PinMode.Output);
power.WriteGpio(Pin.Gpio2, PinValue.High);

power.SetGpioMode(Pin.Gpio0, PinMode.Input);

Debug.WriteLine("M5PM1 initialized. Reporting power telemetry...");

while (true)
{
    Debug.WriteLine($"Battery : {power.GetBatteryVoltage().Millivolts} mV");
    Debug.WriteLine($"VBUS    : {power.GetVbusVoltage().Millivolts} mV");
    Debug.WriteLine($"5V out  : {power.GetOutputVoltage().Millivolts} mV");
    Debug.WriteLine($"Source  : {power.GetPowerSource()}");
    Debug.WriteLine($"Charging: {power.IsCharging}");
    Debug.WriteLine($"GPIO0   : {power.ReadGpio(Pin.Gpio0) == PinValue.High}");
    Debug.WriteLine(string.Empty);

    Thread.Sleep(2000);
}
