// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Adc128D818;
using System;
using System.Device.I2c;
using System.Diagnostics;
using UnitsNet;

Debug.WriteLine("Hello from nanoFramework!");

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
//Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

// set I2C bus ID: 1
// ADC128D818 A0 and A1 pins connected to GND
I2cConnectionSettings settings = new I2cConnectionSettings(1, (int)I2cAddress.Low_Low);
I2cDevice device = I2cDevice.Create(settings);

// pass in I2cDevice
using (Adc128D818 adc = new Adc128D818(device))
{
    // set device to mode 0
    adc.Mode = Mode.Mode0;

    // enable internal voltage reference
    adc.VoltageReference = VoltageReference.Internal2_56;

    // set conversion mode to continuous
    adc.ConversionRate = ConversionRate.Continuous;

    // start conversion
    adc.Start();

    // read raw data from channel IN0
    int channelIn0 = adc.ReadChannel(Channel.In0);

    // output reading
    Console.WriteLine($"Channel IN0 reading: {channelIn0}");

    // read local temperature
    int localTemp = adc.ReadChannel(Channel.Temperature);

    // convert temperature reading 
    Temperature temperature = Adc128D818.ConvertLocalTemperatureReading(localTemp);

    // output temperature
    Console.WriteLine($"Local temperature: {temperature.DegreesCelsius} degC");

    // shutdown the device
    adc.DeepShutdown();
}
