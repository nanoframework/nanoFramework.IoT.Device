// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;
using System.Threading;
using Iot.Device.Lp3943;
using Iot.Device.Lp3943.Samples;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
// Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
// Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new(3, Lp3943.DefaultI2cAddress);
using var device = new I2cDevice(settings);
var resetPin = Utilities.GetPinNumber('D', 10);
///////////////////////////////////////////////////////////////////////
// When using an ESP32 device:
// var resetPin = 32;

using var ledDriver = new Lp3943(device, resetPin);

// Dim LEDs 0 to 7 at 1 Hz at 25% duty cycle
ledDriver.DimRegister(DimRegister.Dim0, 1, 25);

// Dim LEDs 8 to 15 at 5 Hz at 50% duty cycle
ledDriver.DimRegister(DimRegister.Dim1, 5, 50);

// Set LEDs 12, 13, 14, 15 off
ledDriver.SetLed(
    new[]
    {
        12,
        13,
        14,
        15
    },
    LedState.Off
);

// Set LEDs 0, 1, 2, 3, 4, 5 on
ledDriver.SetLed(
    new[]
    {
        0,
        1,
        2,
        3,
        4,
        5
    },
    LedState.On
);

// Set LEDs 6, 7, 8 to be powered by Dim register 0
ledDriver.SetLed(
    new[]
    {
        6,
        7,
        8
    },
    LedState.Dim0
);

// Set LEDs 9, 10, 11 to be powered by Dim register 1
ledDriver.SetLed(9, LedState.Dim1);
ledDriver.SetLed(10, LedState.Dim1);
ledDriver.SetLed(11, LedState.Dim1);

Thread.Sleep(Timeout.Infinite);
