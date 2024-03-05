// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.RgbDiode;
using System.Drawing;

const byte redGpioPin = 25;
const byte greenGpioPin = 27;
const byte blueGpioPin = 26;

// Uncomment for ESP32
// nanoFramework.Hardware.Esp32.Configuration.SetPinFunction(redGpioPin, nanoFramework.Hardware.Esp32.DeviceFunction.PWM1);
// nanoFramework.Hardware.Esp32.Configuration.SetPinFunction(greenGpioPin, nanoFramework.Hardware.Esp32.DeviceFunction.PWM2);
// nanoFramework.Hardware.Esp32.Configuration.SetPinFunction(blueGpioPin, nanoFramework.Hardware.Esp32.DeviceFunction.PWM3);

var pwm = new RgbDiode(redGpioPin, greenGpioPin, blueGpioPin);

// Should display red
pwm.SetColor(255, 0, 0);
// Should display green
pwm.SetColor(0, 255, 0);
// Should display blue
pwm.SetColor(0, 0, 255);

pwm.SetColor(Color.LawnGreen);
pwm.SetColor(Color.OrangeRed);
pwm.SetColor(Color.Teal);

// Will fade blue to green
pwm.Transition(Color.FromArgb(0, 255, 0));
// Will fade green to red
pwm.Transition(Color.FromArgb(255, 0, 0));
