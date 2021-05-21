// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Threading;
using System.Collections.Generic;
using Iot.Device.Bno055;
using Iot.Device.Ft4222;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.PowerMode;
using UnitsNet;

Debug.WriteLine("Hello I2C, SPI and GPIO FTFI! FT4222");
Debug.WriteLine("Select the test you want to run:");
Debug.WriteLine(" 1 Run I2C tests with a BNO055");
Debug.WriteLine(" 2 Run SPI tests with a simple HC595 with led blinking on all ports");
Debug.WriteLine(" 3 Run GPIO tests with a simple led blinking on GPIO2 port and reading the port");
Debug.WriteLine(" 4 Run callback test event on GPIO2 on Failing and Rising");
var key = Console.ReadKey();
Debug.WriteLine();

ListFtDevice devices = FtCommon.GetDevices();
Debug.WriteLine($"{devices.Count} FT4222 elements found");
foreach (FtDevice device in devices)
{
    Debug.WriteLine($"Description: {device.Description}");
    Debug.WriteLine($"Flags: {device.Flags}");
    Debug.WriteLine($"Id: {device.Id}");
    Debug.WriteLine($"Location Id: {device.LocId}");
    Debug.WriteLine($"Serial Number: {device.SerialNumber}");
    Debug.WriteLine($"Device type: {device.Type}");
}

if (devices.Count == 0)
{
    Debug.WriteLine("No devices connected to run tests.");
    return;
}

FtDevice firstDevice = devices[0];

var (chip, dll) = FtCommon.GetVersions();
Debug.WriteLine($"Chip version: {chip}");
Debug.WriteLine($"Dll version: {dll}");

if (key.KeyChar == '1')
{
    TestI2c(firstDevice);
}

if (key.KeyChar == '2')
{
    TestSpi();
}

if (key.KeyChar == '3')
{
    TestGpio();
}

if (key.KeyChar == '4')
{
    TestEvents();
}

void TestI2c(FtDevice device)
{
    using I2cBus ftI2c = device.CreateI2cBus();
    using Bno055Sensor bno055 = new(ftI2c.CreateDevice(Bno055Sensor.DefaultI2cAddress));
    using Bme280 bme280 = new(ftI2c.CreateDevice(Bme280.DefaultI2cAddress));
    bme280.SetPowerMode(Bmx280PowerMode.Normal);

    Debug.WriteLine($"Id: {bno055.Info.ChipId}, AccId: {bno055.Info.AcceleratorId}, GyroId: {bno055.Info.GyroscopeId}, MagId: {bno055.Info.MagnetometerId}");
    Debug.WriteLine($"Firmware version: {bno055.Info.FirmwareVersion}, Bootloader: {bno055.Info.BootloaderVersion}");
    Debug.WriteLine($"Temperature source: {bno055.TemperatureSource}, Operation mode: {bno055.OperationMode}, Units: {bno055.Units}");

    if (bme280.TryReadTemperature(out Temperature temperature))
    {
        Debug.WriteLine($"Temperature: {temperature}");
    }
}

void TestSpi()
{
    using Ft4222Spi ftSpi = new(new SpiConnectionSettings(0, 1) { ClockFrequency = 1_000_000, Mode = SpiMode.Mode0 });

    while (!Console.KeyAvailable)
    {
        ftSpi.WriteByte(0xFF);
        Thread.Sleep(500);
        ftSpi.WriteByte(0x00);
        Thread.Sleep(500);
    }
}

void TestGpio()
{
    const int Gpio2 = 2;
    using GpioController gpioController = new(PinNumberingScheme.Board, new Ft4222Gpio());

    // Opening GPIO2
    gpioController.OpenPin(Gpio2);
    gpioController.SetPinMode(Gpio2, PinMode.Output);

    Debug.WriteLine("Blinking GPIO2");
    while (!Console.KeyAvailable)
    {
        gpioController.Write(Gpio2, PinValue.High);
        Thread.Sleep(500);
        gpioController.Write(Gpio2, PinValue.Low);
        Thread.Sleep(500);
    }

    Console.ReadKey();
    Debug.WriteLine("Reading GPIO2 state");
    gpioController.SetPinMode(Gpio2, PinMode.Input);
    while (!Console.KeyAvailable)
    {
        Console.Write($"State: {gpioController.Read(Gpio2)} ");
        Console.CursorLeft = 0;
        Thread.Sleep(50);
    }
}

void TestEvents()
{
    const int Gpio2 = 2;
    using GpioController gpioController = new(PinNumberingScheme.Board, new Ft4222Gpio());

    // Opening GPIO2
    gpioController.OpenPin(Gpio2);
    gpioController.SetPinMode(Gpio2, PinMode.Input);

    Debug.WriteLine("Setting up events on GPIO2 for rising and failing");

    gpioController.RegisterCallbackForPinValueChangedEvent(Gpio2, PinEventTypes.Falling | PinEventTypes.Rising, MyCallbackFailing);

    Debug.WriteLine("Event setup, press a key to remove the failing event");
    while (!Console.KeyAvailable)
    {
        WaitForEventResult res = gpioController.WaitForEvent(Gpio2, PinEventTypes.Falling, new TimeSpan(0, 0, 0, 0, 50));
        if ((!res.TimedOut) && (res.EventTypes != PinEventTypes.None))
        {
            MyCallbackFailing(gpioController, new PinValueChangedEventArgs(res.EventTypes, Gpio2));
        }

        res = gpioController.WaitForEvent(Gpio2, PinEventTypes.Rising, new TimeSpan(0, 0, 0, 0, 50));
        if ((!res.TimedOut) && (res.EventTypes != PinEventTypes.None))
        {
            MyCallbackFailing(gpioController, new PinValueChangedEventArgs(res.EventTypes, Gpio2));
        }
    }

    Console.ReadKey();
    gpioController.UnregisterCallbackForPinValueChangedEvent(Gpio2, MyCallbackFailing);
    gpioController.RegisterCallbackForPinValueChangedEvent(Gpio2, PinEventTypes.Rising, MyCallback);

    Debug.WriteLine("Event removed, press a key to remove all events and quit");
    while (!Console.KeyAvailable)
    {
        WaitForEventResult res = gpioController.WaitForEvent(Gpio2, PinEventTypes.Rising, new TimeSpan(0, 0, 0, 0, 50));
        if ((!res.TimedOut) && (res.EventTypes != PinEventTypes.None))
        {
            MyCallback(gpioController, new PinValueChangedEventArgs(res.EventTypes, Gpio2));
        }
    }

    gpioController.UnregisterCallbackForPinValueChangedEvent(Gpio2, MyCallback);
}

void MyCallback(object sender, PinValueChangedEventArgs pinValueChangedEventArgs) =>
    Debug.WriteLine($"Event on GPIO {pinValueChangedEventArgs.PinNumber}, event type: {pinValueChangedEventArgs.ChangeType}");

void MyCallbackFailing(object sender, PinValueChangedEventArgs pinValueChangedEventArgs) =>
    Debug.WriteLine($"Event on GPIO {pinValueChangedEventArgs.PinNumber}, event type: {pinValueChangedEventArgs.ChangeType}");
