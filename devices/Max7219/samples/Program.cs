// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Spi;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Max7219;

string message = "Hello World from MAX7219!";

Debug.WriteLine(message);

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the SPI GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.SPI1_MOSI);
//Configuration.SetPinFunction(22, DeviceFunction.SPI1_MISO);
//Configuration.SetPinFunction(22, DeviceFunction.SPI1_CLOCK);

//  Note: the ChipSelect pin should be adjusted to your device, here 42
SpiConnectionSettings connectionSettings = new(1, 42)
{
    ClockFrequency = Iot.Device.Max7219.Max7219.SpiClockFrequency,
    Mode = Iot.Device.Max7219.Max7219.SpiMode
};
using SpiDevice spi = SpiDevice.Create(connectionSettings);
using Max7219 devices = new(spi, cascadedDevices: 4);
// initialize the devices
devices.Init();

// reinitialize the devices
Debug.WriteLine("Init");
devices.Init();

// write a smiley to devices buffer
var smiley = new byte[]
{
    0b00111100,
    0b01000010,
    0b10100101,
    0b10000001,
    0b10100101,
    0b10011001,
    0b01000010,
    0b00111100
};

for (var i = 0; i < devices.CascadedDevices; i++)
{
    for (var digit = 0; digit < 8; digit++)
    {
        devices[new DeviceIdDigit(i, digit)] = smiley[digit];
    }
}

// flush the smiley to the devices using a different rotation each iteration.
//foreach (RotationType rotation in Enum.GetValues(typeof(RotationType)))

Debug.WriteLine($"Send Smiley using rotation {devices.Rotation}.");
devices.Rotation = RotationType.None;
devices.Flush();
Thread.Sleep(1000);
Debug.WriteLine($"Send Smiley using rotation {devices.Rotation}.");
devices.Rotation = RotationType.Right;
devices.Flush();
Thread.Sleep(1000);
Debug.WriteLine($"Send Smiley using rotation {devices.Rotation}.");
devices.Rotation = RotationType.Half;
devices.Flush();
Thread.Sleep(1000);
Debug.WriteLine($"Send Smiley using rotation {devices.Rotation}.");
devices.Rotation = RotationType.Left;
devices.Flush();
Thread.Sleep(1000);

// reinitialize device and show message using the matrix graphics
devices.Init();
devices.Rotation = RotationType.Right;
MatrixGraphics graphics = new(devices, Fonts.Default);
foreach (var font in new[]
{
    Fonts.CP437, Fonts.LCD, Fonts.Sinclair, Fonts.Tiny, Fonts.CyrillicUkrainian
})
{
    graphics.Font = font;
    graphics.ShowMessage(message, alwaysScroll: true);
}

RotationType ReadRotation(char c) => c switch
{
    'l' => RotationType.Left,
    'r' => RotationType.Right,
    'n' => RotationType.None,
    'h' => RotationType.Half,
    _ => RotationType.None,
};
