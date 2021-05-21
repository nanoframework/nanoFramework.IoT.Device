// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Mpr121;

using I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, deviceAddress: Mpr121.DefaultI2cAddress));

// Initialize controller with default configuration and auto-refresh the channel statuses every 100 ms.
using Mpr121 mpr121 = new(i2cDevice: i2cDevice, periodRefresh: 100);

Console.Clear();
Console.CursorVisible = false;

PrintChannelsTable();
Debug.WriteLine("Press Enter to exit.");

// Subscribe to channel statuses updates.
mpr121.ChannelStatusesChanged += (object? sender, ChannelStatusesChangedEventArgs e) =>
    {
        var channelStatuses = e.ChannelStatuses;
        foreach (var channel in channelStatuses.Keys)
        {
            Console.SetCursorPosition(14, (int)channel * 2 + 1);
            Console.Write(channelStatuses[channel] ? "#" : " ");
        }
    };

Console.ReadLine();
Console.Clear();
Console.CursorVisible = true;

void PrintChannelsTable()
{
    Debug.WriteLine("-----------------");

    foreach (var channel in Enum.GetValues(typeof(Channels)))
    {
        Debug.WriteLine("| " + Enum.GetName(typeof(Channels), channel) + " |   |");
        Debug.WriteLine("-----------------");
    }
}
