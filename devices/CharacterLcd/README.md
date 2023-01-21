# Character LCD (Liquid Crystal Display)

This device binding is meant to work with character LCD displays which use a HD44780 compatible controller. Almost all character LCDs fall into this category. Simple wrappers for 16x2 and 20x4 variants are included.

Please make sure you are using the [latest nuget](https://www.nuget.org/packages/nanoFramework.Iot.Device.CharacterLcd).

## Documentation

This binding ~~should work and~~ is in **preview** ~~has been tested~~ with a variety of 16x2 and 20x4 displays both in 4bit and 8bit mode and via i2C adapters (such as on the CrowPi). It should work with any character LCD with a 5x8 size character. Common names are 1602LCD and 2004LCD. Also supports [Grove - LCD RGB Backlight](http://wiki.seeedstudio.com/Grove-LCD_RGB_Backlight/).

- [Very complete tutorial](https://learn.adafruit.com/drive-a-16x2-lcd-directly-with-a-raspberry-pi/overview) on how to connect and work with one of these displays.
- [Good guide](http://www.site2241.net/november2014.htm) explaining how the device works internally
- Seeedstudio, Grove - [LCD RGB Backlight library](https://github.com/Seeed-Studio/Grove_LCD_RGB_Backlight)
- [PCF8574T information](https://alselectro.wordpress.com/2016/05/12/serial-lcd-i2c-module-pcf8574/)

## Usage

These devices are controlled purely by GPIO (except Grove LCD RGB Backlight). There are two different types of GPIO pins that are used, the control pins, and the data pins. The data pins are the ones that will send out the text that should be printed out on the LCD screen. This binding supports two different configurations for the data pins: using 4 data pins, and using 8 data pins. When using only 4 data pins, we will require two send two messages to the controller, each sending half of the byte that should be printed.

Here is a Hello World example of how to consume this binding:

```csharp
            using LcdInterface lcdInterface = LcdInterface.CreateI2c(_i2cDevice, false);
            using Hd44780 lcd = new Lcd1602(lcdInterface);
            lcd.UnderlineCursorVisible = false;
            lcd.Write("Hello World!");
```

Grove LCD RGB Backlight uses two i2c devices:

- the device to control LCD (address 0x3E)
- the device to control RGB backlight (address 0x62)

Make sure the Grove-LCD RGB Backlight is connected to a I2C port. Not the Digital port!
Here is a Hello World example of how to consume Grove LCD RGB Backlight binding:

```csharp
var i2cLcdDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, deviceAddress: 0x3E));
var i2cRgbDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, deviceAddress: 0x62));
using LcdRgb lcd = new LcdRgb(new Size(16, 2), i2cLcdDevice, i2cRgbDevice);
{
    lcd.Write("Hello World!");
    lcd.SetBacklightColor(Color.Azure);
}
```

PCF8574T/PCF8574AT Sample
The I2C backpack based on the PCF8574T/AT IC uses specific pin mapping, to consume this device binding on this backpack use like so

```csharp
var i2cDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, deviceAddress: 0x27));
var controller = new Pcf8574(i2cDevice);
var lcd = new Lcd1602(registerSelectPin: 0, enablePin: 2, dataPins: new int[] { 4, 5, 6, 7}, backlightPin: 3, readWritePin: 1, controller: controller);
```

there is a full working example in the samples directory called Pcf8574tSample.cs
For PCF8574T i2c addresses can be between 0x27 and 0x20 depending on bridged solder jumpers and for PCF8574AT i2c addresses can be between 0x3f and 0x38 depending on bridged solder jumpers

This device binding can be combined with the [ShiftRegister](https://github.com/dotnet/iot/tree/main/src/devices/ShiftRegister/README.md) binding in order to drive an HD44780 display using a shift register. The [ShiftRegister](https://github.com/dotnet/iot/tree/main/src/devices/ShiftRegister/README.md) binding enables interaction via GPIO or SPI. Any shift register can be used as long as it's output length is evenly divisible by 8. Example:

```csharp
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
```

The sample code works with Adafruit's [I2C / SPI character LCD backpack](https://learn.adafruit.com/i2c-spi-lcd-backpack) which uses the [Sn74hc595](https://github.com/dotnet/iot/blob/main/src/devices/Sn74hc595/README.md) 8-bit shift register to support SPI communication. The pin parameters are set according to the backpack's [schematic](https://learn.adafruit.com/i2c-spi-lcd-backpack/downloads).

## Character LCD display Samples

[Different samples](./samples/) are provided. The main method will use the Board's Gpio pins to drive the LCD display. The second example will instead use an MCP Gpio extender backpack to drive the LCD display. Also the second example can use Grove RGB LCD Backlight via i2c bus. This second example has been tested on a CrowPi device and Grove LCD RGB Backlight device.
