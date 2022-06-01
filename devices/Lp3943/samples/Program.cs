// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;
using System.Threading;
using Iot.Device.Lp3943;

I2cConnectionSettings settings = new(3, Lp3943.DefaultI2cAddress);
using var device = new I2cDevice(settings);

using var ledDriver = new Lp3943(device);

ledDriver.DimRegister(DimRegister.Dim0, 1, 25);
ledDriver.DimRegister(DimRegister.Dim1, 5, 50);
ledDriver.SetLed(13, LedState.Off);
ledDriver.SetLed(14, LedState.Off);
ledDriver.SetLed(15, LedState.Off);

Thread.Sleep(Timeout.Infinite);
