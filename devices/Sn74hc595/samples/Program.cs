// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Multiplexing;

using Sn74hc595 sr = new(Sn74hc595PinMapping.Complete);
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

Debug.WriteLine($"Driver for {nameof(Iot.Device.Multiplexing.Sn74hc595)}");
Debug.WriteLine($"Register bit length: {sr.BitLength}");
string interfaceType = sr.UsesSpi ? "SPI" : "GPIO";
Debug.WriteLine($"Using {interfaceType}");

DemonstrateShiftingBytes(sr);
BinaryCounter(sr);

void DemonstrateShiftingBits(Sn74hc595 sr)
{
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

void DemonstrateShiftingBytes(Sn74hc595 sr)
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

    byte lit = 0b_1111_1111; // 255
    Debug.WriteLine($"Write {lit} to each register with {nameof(sr.ShiftByte)}");
    for (int i = 0; i < sr.BitLength / 8; i++)
    {
        sr.ShiftByte(lit);
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
    sr.ShiftByte(56);
    sr.ShiftClear();
}

void BinaryCounter(Sn74hc595 sr)
{
    Debug.WriteLine($"Write 0 through 255");
    for (int i = 0; i < 256; i++)
    {
        sr.ShiftByte((byte)i);
        Thread.Sleep(50);
        sr.ClearStorage();
    }

    sr.ShiftClear();

    if (sr.BitLength > 8)
    {
        Debug.WriteLine($"Write 256 through 4095; pick up the pace");
        for (int i = 256; i < 4096; i++)
        {
            ShiftBytes(sr, i);
            Thread.Sleep(25);
            sr.ClearStorage();
        }
    }

    sr.ShiftClear();
}

void ShiftBytes(Sn74hc595 sr, int value)
{
    if (sr.BitLength > 32)
    {
        throw new Exception($"{nameof(ShiftBytes)}: bit length must be  8-32.");
    }

    for (int i = (sr.BitLength / 8) - 1; i > 0; i--)
    {
        int shift = i * 8;
        int downShiftedValue = value >> shift;
        sr.ShiftByte((byte)downShiftedValue);
    }

    sr.ShiftByte((byte)value);
}
