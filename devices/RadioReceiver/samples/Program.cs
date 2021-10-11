// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.RadioReceiver;
using UnitsNet;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
//Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new(1, Tea5767.DefaultI2cAddress);
using I2cDevice device = I2cDevice.Create(settings);

using Tea5767 radio = new Tea5767(device, FrequencyRange.Other, Frequency.FromMegahertz(103.3));

Thread.Sleep(Timeout.Infinite);

