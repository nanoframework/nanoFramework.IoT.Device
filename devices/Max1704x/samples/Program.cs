// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Max1704x;

namespace Max1704x.samples
{
    public class Program
    {
        public static void Main()
        {
            // Make sure your pins are configured correctly especially for ESP32
            var i2cDevice = I2cDevice.Create(new I2cConnectionSettings(1, Max1704X.DefaultAddress));
            var max = new Max17048(i2cDevice);
            Console.WriteLine($"Voltage: {max.BatteryVoltage.Volts}");
            Console.WriteLine($"Percent: {max.BatteryPercent.Percent} %");
            Thread.Sleep(Timeout.Infinite);
        }
    }
}