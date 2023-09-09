// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Dac63004;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
//Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
//////////////////////////////////////////////////////////////////////

I2cConnectionSettings settings = new(3, Dac63004.DefaultI2cAddress);
using I2cDevice device = I2cDevice.Create(settings);
using Dac63004 dac = new(device);

Debug.WriteLine("");
Debug.WriteLine($"DAC63004 connected to I2C{device.ConnectionSettings.BusId}");
Debug.WriteLine("");

Debug.WriteLine("Set channels 0 and 1 to volrage output");

dac.ConfigureChannelMode(Channel.Channel0, Mode.VoltageOutput);
dac.ConfigureChannelMode(Channel.Channel1, Mode.VoltageOutput);

Debug.WriteLine("Enabling internal reference");
dac.InternalRefEnabled = true;

Debug.WriteLine("Setting channel 0 to 1.5x gain and channel 1 to 2x gain");
dac.ConfigureChannelVoutGain(Channel.Channel0, VoutGain.Internal1_5x);
dac.ConfigureChannelVoutGain(Channel.Channel1, VoutGain.Internal2x);

int dacValue = 100;

while (true)
{
    Debug.WriteLine($"Setting channel data to {dacValue}");

    dac.SetChannelDataValue(Channel.Channel0, dacValue);
    dac.SetChannelDataValue(Channel.Channel1, dacValue);

    Debug.WriteLine("");

    dacValue += 100;

    // wrap around
    if (dacValue > 4095)
    {
        dacValue = 100;
    }

    Thread.Sleep(3000);
}
