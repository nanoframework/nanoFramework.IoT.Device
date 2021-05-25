// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.RadioTransmitter;

I2cConnectionSettings settings = new(1, Kt0803.DefaultI2cAddress);
using I2cDevice device = I2cDevice.Create(settings);

using Kt0803 radio = new(device, 106.6, Region.China);
Debug.WriteLine($"The radio is running on FM {radio.Frequency.ToString("0.0")}MHz");

Thread.Sleep(Timeout.Infinite);
