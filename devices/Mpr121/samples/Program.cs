// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Mpr121;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
//Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

using I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, deviceAddress: Mpr121.DefaultI2cAddress));

// Initialize controller with default configuration and auto-refresh the channel statuses every 100 ms.
using Mpr121 mpr121 = new(i2cDevice: i2cDevice, periodRefresh: 100);

// Subscribe to channel statuses updates.
mpr121.ChannelStatusesChanged += (object? sender, ChannelStatusesChangedEventArgs e) =>
    {
        var channelStatuses = e.ChannelStatuses;
        for(int i=0; i<channelStatuses.Length;i++)
        {
            Debug.WriteLine(channelStatuses[i] ? $"{i} #" : $"{i} ");
        }
    };

Thread.Sleep(Timeout.Infinite);