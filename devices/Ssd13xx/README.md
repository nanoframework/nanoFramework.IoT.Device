﻿# SSD13xx & SSH1106 OLED display family

The SSD1306/SSH1106 are a single-chip CMOS OLED/PLED driver with controllers for organic/polymer light emitting diode dot-matrix graphic display system. It consists of 128 segments and 64 commons. This IC is designed for Common Cathode type OLED panel.

## Documentation

- IoT NanoFramework SSD1306/SSH1106 [Driver](https://github.com/nanoframework/nanoFramework.IoT.Device)
- SSD1306 [datasheet](https://cdn-shop.adafruit.com/datasheets/SSD1306.pdf)
- SSD1327 [datasheet](https://github.com/SeeedDocument/Grove_OLED_1.12/raw/master/resources/SSD1327_datasheet.pdf)

### Related Devices

- [Adafruit PiOLED - 128x32 Monochrome OLED Add-on for Raspberry Pi](https://www.adafruit.com/product/3527)
- [HiLetgo 1.3" SPI 128x64 SSH1106 OLED LCD Display LCD](https://www.amazon.com/HiLetgo-128x64-SSH1106-Display-Arduino/dp/B01N1LZT8L/ref=sr_1_2?crid=C88G1YX0AN3Q&dchild=1&keywords=ssh1106&qid=1634064423&sr=8-2)
- [SunFounder 0.96" Inch Blue I2C IIC Serial 128x64 OLED LCD LED SSD1306 Modul](https://www.amazon.com/SunFounder-SSD1306-Arduino-Raspberry-Display/dp/B014KUB1SA)
- [Diymall 0.96" Inch Blue and Yellow I2c IIC Serial Oled LCD LED Module 12864 128X64 for Arduino Display Raspberry PI 51 Msp420 Stim32 SCR](https://www.amazon.com/Diymall-Yellow-Arduino-Display-Raspberry/dp/B00O2LLT30)

## Board

![Connection schematics](https://raw.githubusercontent.com/nanoframework/nanoFramework.IoT.Device/develop/devices/Ssd13xx/Ssd1306_I2c_PiOled.png)

## Binding Notes

This binding currently only supports commands and raw data.  Eventually, the plan is to create a graphics library that can send text and images to the device. So this library is just a start and you'll find in the [sample](./samples) more advance commands.

The following connection types are supported by this binding.

- [X] I2C
- [ ] SPI

## Usage notes

There are two groups of drawing methods.

1. Various specialized drawing methods allowing to draw on screen pixel-by-pixel, like:
    - ````DrawPixel(...)````: draws one pixel
    - ````DrawHorizontalLine(...)````: draws a horizontal line
    - ````DrawVerticalLine(...)````: draws a vertical line
    - ````DrawFilledRectangle(...)````: draws a filled rectangle
    - ````DrawBitmap(...)````: draws a bitmap
    - ````DrawString(...)````: draws a string with preset font
    
    Using these methods you do not need to care about any technique the driver uses to display 
    your drawing instructions.
   
2. Methods allowing to modify screen content by blocks of internal representation (screen buffer), like:
    - ````DrawDirectAligned(...)````: overwrites screen buffer with given content
    - ````ClearDirectAligned(...)````: clears out (with 0x00) given part of screen buffer
    
    These methods allow faster (~100 times) display access but with some constraints. 
    - bitmaps handed over here must be in appropriate format (see SSD13xx docs for "GDDRAM" and "Horizontal addressing mode").
    - no bit operations occure with existing buffer data (with pixels drawn via other means), the new data will overwrite the pixels "below" newly drawed content.
    - the "y" coordinate and the bitmap height must be byte aligned with screen buffer (again, see above docs)

The use of two groups can be freely mixed (e.g. display text via ````DrawString(...)```` and displaying an image below via ````DrawDirectAligned(...)````)

Examples for 1. can be found in ````samples```` folder.

Example for 2. follows here.

````csharp
// There are superb online helpers like the one below which are able to
// create an appropriate byte array from an image in code use ready format.
// https://www.mischianti.org/images-to-byte-array-online-converter-cpp-arduino/
// On the site above use these settings to get bytes needed here:
// - "plain bytes"
// - "vertical - 1 bit per pixel"
var buffer = new byte[] { ... }; 
var width = 16;
var height = 16;

// instantiation example
var ssd1306 = new Ssd1306(
    I2cDevice.Create(
        new I2cConnectionSettings(
            1, 
            Ssd1306.DefaultI2cAddress, 
            I2cBusSpeed.FastMode)), 
    Ssd13xx.DisplayResolution.OLED128x64);

// this line sends the image data to the screen
ssd1306.DrawDirectAligned(x, y, width, height, buffer);

// this one wipes its place to blank
ssd1306.ClearDirectAligned(x, y, width, height);

````