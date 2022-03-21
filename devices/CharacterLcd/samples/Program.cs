// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// !!!----------- SAMPLE - ENSURE YOU CHOOSE THE CORRECT TARGET HERE --------------!!!
#define ESP32 //Comment this out for any other non ESP32 based target and remove Hardware.ESP32 only nuget!
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
#if ESP32
using nanoFramework.Hardware.Esp32;
#endif
using Iot.Device.Pcx857x;
using SixLabors.ImageSharp;

#if ESP32
// For ESP32, set the pin functions.
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
#endif

// Choose the right setup for your display:
// UsingGpioPins();
UsingGroveRgbDisplay();
// UsingHd44780OverI2C();
// UsingShiftRegister();
// UsingHd44780OverPcf8574();

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
    using I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(3, 0x27)); //Orpal3 uses BusId 3...
    using Pcf8574 controller = new Pcf8574(i2cDevice); //TODO: how would this be used to create LcdInterface?!
    using LcdInterface lcdInterface = LcdInterface.CreateI2c(i2cDevice, false); 
    using Hd44780 lcd = new Lcd1602(lcdInterface);
    {
        lcd.UnderlineCursorVisible = false;
        lcd.BacklightOn = true;
        lcd.DisplayOn = true;
        lcd.Clear();
        Debug.WriteLine("Display initialized.");
        lcd.Write("Hello World!");
        //LcdConsoleSamples.WriteTest(lcd);
        //ExtendedSample.Test(lcd);
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
