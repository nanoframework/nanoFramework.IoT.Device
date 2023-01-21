// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// !!!----------- SAMPLE - ENSURE YOU CHOOSE THE CORRECT TARGET HERE --------------!!!
//#define BUIID_FOR_ESP32 //Comment this out for any non ESP32 based MCU's.
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
Thread.Sleep(2000); // Only for debug (deploy break in) reliability...
UsingHd44780OverPcf8574();
#endif

/// <summary>
/// Use an "Lcd1602[Hd44780O]" via a "GPIO" interface.
/// </summary>
static void UsingGpioPins()
{
    using Lcd1602 lcd = new Lcd1602(registerSelectPin: 22, enablePin: 17, dataPins: new int[] { 25, 24, 23, 18 });
    lcd.Clear();
    lcd.Write("Hello World");
}

/// <summary>
/// Use an "Lcd2604[Hd44780O]" via an "I2C" interface.
/// </summary>
static void UsingHd44780OverI2C()
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

/// <summary>
/// Use an "Lcd1602[Hd44780O]" via a "Pcf8574" interface.
/// </summary>
/// <remarks>
/// This function is currently using the target "OrgPalThree".
/// It will need adjusting for other target boards!
/// </remarks>
static void UsingHd44780OverPcf8574()
{
    // The "OrgPalThree" requires powering on...
    using var lcdGpioController = new GpioController();
    {
        var enablePin = lcdGpioController.OpenPin(PortPin('K', 3), PinMode.Output); //TODO: this should actually be set via `lcd.DisplayOn` using an (non default) `enablePin`?!
        enablePin.Write(PinValue.High); // and this "should be part" of the `Pcf8574` driver!

        using I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(3, 0x3F, I2cBusSpeed.FastMode)); //OrgPalThree uses BusId 3... with a display of device address of `0x3F`
        {
            //using Pcf8574 controller = new Pcf8574(device: i2cDevice, gpioController: lcdGpioController); //TODO: how would this be used to create LcdInterface as nF does not support `GpioDriver` in the `GpioController` constructor!  
            // Using an I2C interface in 4bit mode
            using LcdInterface lcdInterface = LcdInterface.CreateI2c(i2cDevice, false); //LcdInterface.CreateGpio(registerSelectPin: 0, enablePin: PortPin('K', 3), dataPins: new int[] { 4, 5, 6, 7 }, backlightPin: 3, readWritePin: 1, controller: new GpioController());
            {
                using Hd44780 lcd = new Lcd1602(lcdInterface);
                {
                    //lcd.DisplayOn = true; // commented out as would possibily cause issues until `Pcf8574` works!
                    lcd.UnderlineCursorVisible = false;
                    lcd.BacklightOn = true;
                    Debug.WriteLine("Display initialized.");
                    // For debug, lets run some simple commands until they work sucessfully!
                    using LcdConsole display = new LcdConsole(lcd, "A00");
                    {
                        for ( ; ; )
                        {
                            display.Clear();
                            Debug.WriteLine("Writing: 'Hello World !!!'.");
                            display.WriteLine("Hello World !!!");
                            Thread.Sleep(3000);
                            display.BacklightOn = false;
                            Thread.Sleep(3000);
                            display.BacklightOn = true;
                            display.Clear();
                            Debug.WriteLine("Writing: 'From .Net nF :-)'.");
                            display.WriteLine("From .Net nF :-)");
                            Thread.Sleep(3000);
                            display.Clear();
                            Debug.WriteLine("Writing: 'ON YOUR DISPLAY!'.");
                            display.WriteLine("ON YOUR DISPLAY!");
                            Thread.Sleep(3000);
                            display.BacklightOn = false;
                            Thread.Sleep(3000);
                            display.BacklightOn = true;
                            display.Clear();
                            Debug.WriteLine("Writing: 'LCD Hello World!\r\nnanoFramework!'.");
                            //display.WriteLine($"LCD Hello World 4!\r\nnanoFramework!");  //TODO: Currently using `\r\n` produces unwanted chars...
                            display.WriteLine("LCD Hello World!");
                            display.WriteLine("nanoFramework!");
                            Thread.Sleep(3000);
                            //LcdConsoleSamples.WriteTest(lcd);
                            //ExtendedSample.Test(lcd);
                        }
                    }
                }
            }
        }
    }
    Thread.Sleep(Timeout.Infinite);
}

/// <summary>
/// Use an "LcdRgb" display via an "I2C" interface.
/// </summary>
static void UsingGroveRgbDisplay()
{
    var i2cLcdDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, deviceAddress: 0x3E));
    var i2cRgbDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, deviceAddress: 0x62));
    using LcdRgb lcd = new LcdRgb(new Size(16, 2), i2cLcdDevice, i2cRgbDevice);
    {
        lcd.Write("Hello World!");
        lcd.SetBacklightColor(Color.Azure);
    }
}

/// <summary>
/// Use an "Lcd1602[Hd44780O]" via an "SPI" "ShiftRegister" interface.
/// </summary>
static void UsingShiftRegister()
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
