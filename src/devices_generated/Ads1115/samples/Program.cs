// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using System.Device.Gpio;
using Iot.Device.Ads1115;
using UnitsNet;

// set I2C bus ID: 1
// ADS1115 Addr Pin connect to GND
I2cConnectionSettings settings = new(1, (int)I2cAddress.GND);
// get I2cDevice (in Linux)
I2cDevice device = I2cDevice.Create(settings);

Debug.WriteLine("Press any key to continue");
// pass in I2cDevice
// repeatedly measure the voltage AIN0
// set the maximum range to 4.096V
using (Iot.Device.Ads1115.Ads1115 adc = new Iot.Device.Ads1115.Ads1115(device, InputMultiplexer.AIN0, MeasuringRange.FS4096))
{
    while (!Console.KeyAvailable)
    {
        // read raw data form the sensor
        short raw = adc.ReadRaw();
        // raw data convert to voltage
        ElectricPotential voltage = adc.RawToVoltage(raw);

        Debug.WriteLine($"ADS1115 Raw Data: {raw}");
        Debug.WriteLine($"Voltage: {voltage}");
        Debug.WriteLine();

        // wait for 2s
        Thread.Sleep(2000);
    }

    Console.ReadKey(true);
}

// Read all channels in a loop.
// We set the device mode to power-down, because we have to wait for a sample after each channel swap anyway.
using (var adc = new Iot.Device.Ads1115.Ads1115(device, InputMultiplexer.AIN0, MeasuringRange.FS4096, DataRate.SPS250, DeviceMode.PowerDown))
{
    while (!Console.KeyAvailable)
    {
        Console.Clear();

        ElectricPotential voltage0 = adc.ReadVoltage(InputMultiplexer.AIN0);
        ElectricPotential voltage1 = adc.ReadVoltage(InputMultiplexer.AIN1);
        ElectricPotential voltage2 = adc.ReadVoltage(InputMultiplexer.AIN2);
        ElectricPotential voltage3 = adc.ReadVoltage(InputMultiplexer.AIN3);

        Debug.WriteLine($"ADS1115 Voltages: (Any key to continue)");
        Debug.WriteLine($"Channel0: {voltage0:s3}");
        Debug.WriteLine($"Channel1: {voltage1:s3}");
        Debug.WriteLine($"Channel2: {voltage2:s3}");
        Debug.WriteLine($"Channel3: {voltage3:s3}");
        Debug.WriteLine();

        // wait for 100ms
        Thread.Sleep(100);
    }

    while (Console.KeyAvailable)
    {
        Console.ReadKey(true);
    }
}

// Provide a callback that triggers each time the ADC has a new value available. The DataRate parameter will define the sample rate.
// We are using pin 23 as interrupt input from the ADC, but note that the trigger signal from the ADC may be to short to be properly recognized by the Raspberry Pi and
// some extra electronics is required to make this reliably work (see readme).
using (var controller = new GpioController(PinNumberingScheme.Logical))
{
    Console.Clear();
    Debug.WriteLine("This triggers an interrupt each time a new value is available on AIN0");
    using Iot.Device.Ads1115.Ads1115 adc = new Iot.Device.Ads1115.Ads1115(device, controller, 23, false, InputMultiplexer.AIN0, MeasuringRange.FS2048, DataRate.SPS250);
    Stopwatch w = Stopwatch.StartNew();
    int totalInterruptsSeen = 0;
    int previousNumberOfInterrupts = 0;
    ElectricPotential lastVoltage = default;
    adc.AlertReadyAsserted += () =>
    {
        ElectricPotential voltage = adc.ReadVoltage();
        lastVoltage = voltage;
        totalInterruptsSeen++;
    };

    adc.EnableConversionReady();
    // (Do something else, here we print the output (as the console operations use to much time in the interrupt callback)
    while (!Console.KeyAvailable)
    {
        int interruptsThisPeriod = totalInterruptsSeen - previousNumberOfInterrupts;
        double intsSecond = interruptsThisPeriod / w.Elapsed.TotalSeconds;

        Debug.WriteLine($"ADS1115 Voltage: {lastVoltage}");
        Debug.WriteLine($"Interrups total: {totalInterruptsSeen}, last: {interruptsThisPeriod} Average: {intsSecond}/s (should be ~ {adc.FrequencyFromDataRate(adc.DataRate)})");
        w.Restart();
        previousNumberOfInterrupts = totalInterruptsSeen;
        // wait for 2s
        Thread.Sleep(2000);
    }

    Console.ReadKey(true);
}

// Use an interrupt handler, but this time when the value on AIN1 exceeds a threshold
using (var controller = new GpioController(PinNumberingScheme.Logical))
{
    Console.Clear();
    Debug.WriteLine("This triggers an interrupt as long as the value is above 2.0V (and then stays above 1.8V)");
    using Iot.Device.Ads1115.Ads1115 adc = new Iot.Device.Ads1115.Ads1115(device, controller, 23, false, InputMultiplexer.AIN1, MeasuringRange.FS4096, DataRate.SPS860);
    Stopwatch w = Stopwatch.StartNew();
    int totalInterruptsSeen = 0;
    int previousNumberOfInterrupts = 0;
    ElectricPotential lastVoltage = default;
    adc.AlertReadyAsserted += () =>
    {
        ElectricPotential voltage = adc.ReadVoltage();
        lastVoltage = voltage;
        totalInterruptsSeen++;
    };

    adc.EnableComparator(adc.VoltageToRaw(ElectricPotential.FromVolts(1.8)), adc.VoltageToRaw(ElectricPotential.FromVolts(2.0)), ComparatorMode.Traditional, ComparatorQueue.AssertAfterTwo);
    // Do something else, here we print the output (as the console operations use to much time in the interrupt callback)
    while (!Console.KeyAvailable)
    {
        int interruptsThisPeriod = totalInterruptsSeen - previousNumberOfInterrupts;
        double intsSecond = interruptsThisPeriod / w.Elapsed.TotalSeconds;

        if (interruptsThisPeriod > 0)
        {
            Debug.WriteLine($"Interrupt voltage: {lastVoltage}");
            Debug.WriteLine($"Interrups total: {totalInterruptsSeen}, last: {interruptsThisPeriod} Average: {intsSecond}/s");
        }
        else
        {
            Debug.WriteLine($"Current Voltage (no interrupts seen): {adc.ReadVoltage()}");
        }

        lastVoltage = default;
        w.Restart();
        previousNumberOfInterrupts = totalInterruptsSeen;
        // wait for 2s
        Thread.Sleep(2000);
    }

    Console.ReadKey(true);
}
