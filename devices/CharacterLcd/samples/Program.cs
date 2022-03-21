// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// !!!----------- SAMPLE - ENSURE YOU CHOOSE THE CORRECT TARGET HERE --------------!!!
//#define BUIID_FOR_ESP32 //Comment this out for any non ESP32 based target.
// !!!-----------------------------------------------------------------------------!!!

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using CharacterLcd.Samples;
using Iot.Device.CharacterLcd;
using Iot.Device.CharacterLcd.Samples;
using Iot.Device.Multiplexing;
#if BUIID_FOR_ESP32
using nanoFramework.Hardware.Esp32;
#endif
using Iot.Device.Pcx857x;
using SixLabors.ImageSharp;

#if BUIID_FOR_ESP32
// For ESP32, set the pin functions.
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
#endif

// Choose the right setup for your display:
#if BUIID_FOR_ESP32  //For ease whilst testing against different targets...
// UsingGpioPins();
UsingGroveRgbDisplay();
// UsingHd44780OverI2C();
// UsingShiftRegister();
#else
UsingHd44780OverPcf8574();
#endif

void UsingGpioPins()
{
    using Lcd1602 lcd = new Lcd1602(registerSelectPin: 22, enablePin: 17, dataPins: new int[] { 25, 24, 23, 18 });
    lcd.Clear();
    lcd.Write("Hello World");
}

void UsingHd44780OverI2C()
{
    using I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(1, 0x27));
    using LcdInterface lcdInterface = LcdInterface.CreateI2c(i2cDevice, false);
    using Hd44780 hd44780 = new Lcd2004(lcdInterface);
    hd44780.UnderlineCursorVisible = false;
    hd44780.BacklightOn = true;
    hd44780.DisplayOn = true;
    hd44780.Clear();
    Debug.WriteLine("Display initialized.");
    LcdConsoleSamples.WriteTest(hd44780);
    ExtendedSample.Test(hd44780);
}

void UsingHd44780OverPcf8574()
{
    // The orgPal3 requires powering on...
    using var lcdPowerOnOff = new GpioController().OpenPin(PortPin('K', 3), PinMode.Output); //TODO: this should actually be set via `lcd.DisplayOn` using an (non default) `enablePin`?!
    {
        lcdPowerOnOff.Write(PinValue.High);

        using I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(3, 0x3F, I2cBusSpeed.FastMode)); //Orpal3 uses BusId 3...
        {
            //using Pcf8574 controller = new Pcf8574(i2cDevice); //TODO: how would this be used to create LcdInterface as nF does not support `GpioDriver` in the `GpioController` constructor!  
            // Using an I2C interface in 4bit mode
            using LcdInterface lcdInterface = LcdInterface.CreateI2c(i2cDevice, false); //LcdInterface.CreateGpio(registerSelectPin: 0, enablePin: 2, dataPins: new int[] { 4, 5, 6, 7 }, backlightPin: 3, readWritePin: 1, controller: new GpioController(PinNumberingScheme.Logical));
            {
                using Hd44780 lcd = new Lcd1602(lcdInterface);
                {
                    lcd.UnderlineCursorVisible = false;
                    lcd.BacklightOn = true;
                    lcd.DisplayOn = true;
                    lcd.Clear();
                    Debug.WriteLine("Display initialized.");
                    Debug.WriteLine("Writing: 'Hello World!'.");
                    lcd.Write("Hello World!");
                    Thread.Sleep(1000);
                    //lcd.BacklightOn = false;
                    Thread.Sleep(1000);
                    //lcd.BacklightOn = true;
                    lcd.Home();
                    Debug.WriteLine("Writing: 'Hello World2!!!'.");
                    lcd.Write("Hello World 2!!!"); // This (seems to) append!
                    //lcd.BacklightOn = false;
                    Thread.Sleep(1000);
                    //lcd.BacklightOn = true;
                    //lcd.Clear();
                    lcd.SetCursorPosition(0, 0);
                    Debug.WriteLine("Writing: 'Hello World3!'.");
                    lcd.Write("Hello World 3!");
                    //lcd.BacklightOn = false;
                    Thread.Sleep(1000);
                    //lcd.BacklightOn = true;
                    //lcd.Clear();
                    lcd.Home();
                    Debug.WriteLine("Writing: 'Hello World 4!\r\nFrom nanoFramework!'.");
                    lcd.Write("Hello World 4!\r\nFrom nanoFramework!");
                    //LcdConsoleSamples.WriteTest(lcd);
                    //ExtendedSample.Test(lcd);
                }
            }
        }
    }
    Thread.Sleep(Timeout.Infinite);
}

void UsingGroveRgbDisplay()
{
    var i2cLcdDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, deviceAddress: 0x3E));
    var i2cRgbDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, deviceAddress: 0x62));
    using LcdRgb lcd = new LcdRgb(new Size(16, 2), i2cLcdDevice, i2cRgbDevice);
    {
        lcd.Write("Hello World!");
        lcd.SetBacklightColor(Color.Azure);
    }
}

void UsingShiftRegister()
{
    int registerSelectPin = 1;
    int enablePin = 2;
    int[] dataPins = new int[] { 6, 5, 4, 3 };
    int backlightPin = 7;

    // Gpio
    using ShiftRegister sr = new(ShiftRegisterPinMapping.Minimal, 8);

    // Spi
    // using SpiDevice spiDevice = SpiDevice.Create(new(0, 0));
    // using ShiftRegister sr = new(spiDevice, 8);
    using LcdInterface lcdInterface = LcdInterface.CreateFromShiftRegister(registerSelectPin, enablePin, dataPins, backlightPin, sr);
    using Lcd1602 lcd = new(lcdInterface);
    lcd.Clear();
    lcd.Write("Hello World");
}

/// <summary>
/// Used for STM32 devices...
/// </summary>
static int PortPin(char port, byte pin)
{
    if (port < 'A' || port > 'K')
        throw new ArgumentException("Invalid Port definition");

    return ((port - 'A') * 16) + pin;

}
