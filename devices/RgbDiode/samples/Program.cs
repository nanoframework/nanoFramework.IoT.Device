// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.RgbDiode;

const byte redGpioPin = 25;
const byte greenGpioPin = 27;
const byte blueGpioPin = 26;

var pwm = new RgbDiode(redGpioPin, greenGpioPin, blueGpioPin);
pwm.SetColor(255, 0, 0); // Should display red
pwm.SetColor(0, 255, 0); // Should display green
pwm.SetColor(0, 0, 255); // Should display blue
