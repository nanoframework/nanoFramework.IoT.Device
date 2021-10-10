// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Mcp3428;

Debug.WriteLine("Hello Mcp3428 Sample!");

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
//Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings options = new(1, Mcp3428.I2CAddressFromPins(PinState.Low, PinState.Low));
using I2cDevice i2cDevice = I2cDevice.Create(options);
using Mcp3428 adc = new(i2cDevice, AdcMode.OneShot, resolution: AdcResolution.Bit16, pgaGain: AdcGain.X1);
var watch = new Stopwatch();
watch.Start();
while (true)
{
    for (int i = 0; i < 4; i++)
    {
        var last = watch.ElapsedMilliseconds;
        var value = adc.ReadChannel(i);

        foreach (var b in adc.LastBytes)
        {
            Debug.Write($"{b:X} ");
        }

        Debug.WriteLine("");
        Debug.WriteLine($"ADC Channel[{adc.LastChannel + 1}] read in {watch.ElapsedMilliseconds - last} ms, value: {value} V");
        Thread.Sleep(500);
    }

    Debug.WriteLine($"mode {adc.Mode}, gain {adc.InputGain}, res {adc.Resolution}");
    Thread.Sleep(1000);
}
