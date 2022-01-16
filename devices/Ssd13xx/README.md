# SSD13xx & SSH1106 OLED display family

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

![Connection schematics](Ssd1306_I2c_PiOled.png)

## Binding Notes

This binding currently only supports commands and raw data.  Eventually, the plan is to create a graphics library that can send text and images to the device. So this library is just a start and you'll find in the [sample](./samples) more advance commands.

The following connection types are supported by this binding.

- [X] I2C
- [ ] SPI

## Performance suggestions

If you wish to update big part of screen (e.g. drawing images or shapes instead of printing text) 
then you may find that API methods like ````DrawFilledRectangle(...)```` or ````DrawBitmap(...)````
not performing fast enough. These methods are using ````DrawPixel(...)```` method internally which means
the expected drawing task will be accomplished via pixel-by-pixel drawing.

There are two methods to overcome this problem if you have ready bitmaps to display.

Assuming the bitmaps are in appropriate format (see SSD13xx docs for "GDDRAM" and "Horizontal addressing mode")
the ````DrawDirectAligned(...)```` method simply copies the incoming byte array to appropriate place in internal buffer
which results is ~100 times faster display speed.
There are constraints: no bit operations occure with existing buffer data (pixels drawn via other means), 
so the "y" coordinate and the bitmap height must be byte aligned!

Same constraints apply to ````ClearDirectAligned(...)```` method which allow partial (rectangle) screen clearing
by setting appropriate internal buffer bytes to 0x00. This is also ~100 times faster than doing same with ````DrawFilledRectangle(...)```` method.

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