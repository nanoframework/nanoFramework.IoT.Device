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
ledDriver.SetLed(new [] { 12, 13, 14, 15 }, LedState.Off);
ledDriver.SetLed(new[] { 0, 1, 2, 3, 4, 5 }, LedState.On);
ledDriver.SetLed(new[] { 6, 7, 8 }, LedState.Dim0);
ledDriver.SetLed(9, LedState.Dim1);
ledDriver.SetLed(10, LedState.Dim1);
ledDriver.SetLed(11, LedState.Dim1);

Thread.Sleep(Timeout.Infinite);
