// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Multiplexing;

using ShiftRegister sr = new ShiftRegister(ShiftRegisterPinMapping.Complete, 8);

// Uncomment this code to use SPI (and comment the line above)
//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the SPI GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.SPI1_MOSI);
//Configuration.SetPinFunction(22, DeviceFunction.SPI1_MISO);
//Configuration.SetPinFunction(23, DeviceFunction.SPI1_CLOCK);
// Make sure as well you are using the right chip select
// SpiConnectionSettings settings = new(1, 42);
// using var spiDevice = SpiDevice.Create(settings);
// var sr = new Sn74hc595(spiDevice, Sn74hc595.PinMapping.Standard);
CancellationTokenSource cancellationSource = new();

Debug.WriteLine($"Driver for {nameof(ShiftRegister)}");
Debug.WriteLine($"Register bit length: {sr.BitLength}");
string interfaceType = sr.UsesSpi ? "SPI" : "GPIO";
Debug.WriteLine($"Using {interfaceType}");

sr.OutputEnable = true;

DemonstrateShiftingBits(sr);
DemonstrateShiftingBytes(sr);
BinaryCounter(sr);
Debug.WriteLine("done");

void DemonstrateShiftingBits(ShiftRegister sr)
{
    if (sr.UsesSpi)
    {
        return;
    }

    int delay = 1000;
    sr.ShiftClear();

    Debug.WriteLine("Light up three of first four LEDs");
    sr.ShiftBit(1);
    sr.ShiftBit(1);
    sr.ShiftBit(0);
    sr.ShiftBit(1);
    sr.Latch();
    Thread.Sleep(delay);

    sr.ShiftClear();

    Debug.WriteLine($"Light up all LEDs, with {nameof(sr.ShiftBit)}");

    for (int i = 0; i < sr.BitLength; i++)
    {
        sr.ShiftBit(1);
    }

    sr.Latch();
    Thread.Sleep(delay);

    sr.ShiftClear();

    Debug.WriteLine($"Dim up all LEDs, with {nameof(sr.ShiftBit)}");

    for (int i = 0; i < sr.BitLength; i++)
    {
        sr.ShiftBit(0);
    }

    sr.Latch();
    Thread.Sleep(delay);
}

void DemonstrateShiftingBytes(ShiftRegister sr)
{
    int delay = 1000;
    Debug.WriteLine($"Write a set of values with {nameof(sr.ShiftByte)}");
    // this can be specified as ints or binary notation -- its all the same
    var values = new byte[] { 0b1, 23, 56, 127, 128, 170, 0b_1010_1010 };
    foreach (var value in values)
    {
        Debug.WriteLine($"Value: {value}");
        sr.ShiftByte(value);
        Thread.Sleep(delay);
        sr.ShiftClear();
    }

    byte litPattern = 0b_1111_1111; // 255
    Debug.WriteLine($"Write {litPattern} to each register with {nameof(sr.ShiftByte)}");
    for (int i = 0; i < sr.BitLength / 8; i++)
    {
        sr.ShiftByte(litPattern);
    }

    Thread.Sleep(delay);

    Debug.WriteLine("Output disable");
    sr.OutputEnable = false;
    Thread.Sleep(delay * 2);

    Debug.WriteLine("Output enable");
    sr.OutputEnable = true;
    Thread.Sleep(delay * 2);

    Debug.WriteLine($"Write 23 then 56 with {nameof(sr.ShiftByte)}");
    sr.ShiftByte(23);
    Thread.Sleep(delay);
    sr.ShiftByte(56);
    sr.ShiftClear();
}

void BinaryCounter(ShiftRegister sr)
{
    Debug.WriteLine($"Write 0 through 255");
    for (int i = 0; i < 256; i++)
    {
        sr.ShiftByte((byte)i);
        Thread.Sleep(50);
        sr.ShiftClear();
    }

    sr.ShiftClear();

    if (sr.BitLength > 8)
    {
        Debug.WriteLine($"Write 256 through 4095; pick up the pace");
        for (int i = 256; i < 4096; i++)
        {
            ShiftBytes(sr, i);
            Thread.Sleep(20);
        }
    }

    sr.ShiftClear();
}

void ShiftBytes(ShiftRegister sr, int value)
{
    if (sr.BitLength > 32)
    {
        throw new Exception($"{nameof(ShiftBytes)}: bit length must be  8-32.");
    }

    for (int i = (sr.BitLength / 8) - 1; i > 0; i--)
    {
        int shift = i * 8;
        int downShiftedValue = value >> shift;
        sr.ShiftByte((byte)downShiftedValue, false);
    }

    sr.ShiftByte((byte)value);
}

