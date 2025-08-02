# XPT2046 - Touch screen controller

The XPT2046 from XPTEK is a 4-wire resistive touch screen controller that incorporates a 12-bit 125 kHz successive approximation register type A/D converter. The XPT2046 can detect the pressed screen location by performing two A/D conversions. As well as the location, the XPT2046 also measures touch screen pressure. A multiplexer allows it to measure chip temperature and battery voltage.

The XPT2046 is common in a number of touch screen modules from suppliers like WaveShare and an number of ESP32 based display boards.

Communication is via the SPI bus which can communicate at maximum speed of 2Mhz and uses Mode 0 for the clock polarity/phase setting. The host sends an 8 bit command and the chip responds with 16 bits of data althought the first bit and last three bits are not part of the conversion. XPT2046 needs 16 clock cycles to read a value, so this driver sends 8 bits and a padding byte between each command, and the device can be reading one result whilst sending the command for the next measurement. When reading we need to skip the first 8 bits.

An additional interupt pin which is usually in a high state and goes low when a touch is detected. Note that communicating with the chip will interfere with the interupt.

The chip can use an internal voltage reference or be configured to use an external reference if more accuracy is required.

The XPT2046 will enter power down mode and stop communicating when the chip select pin is taken high. This is why TransferFullDuplex is used as the framework does not expose the raw SPI transaction.

The driver does a best of 3 average, thanks Paul Stoffregen for the suggestion via your Arduino library.

## Documentation

Datasheet - https://www.waveshare.com/wiki/File:XPT2046-EN.pdf

## Usage

See TouchDemo for a complete example of usage based on the ESP32 and a cheap yellow display module.

The driver also supports a maximum X and Y value so you can map the output to your screen size.

The driver scales the output based on a maximum X and Y value so you can map it to pixels on your screen.

**Important**: For the ESP32 the default pins can be remapped, do so before creating the `SPIDevice`.

```csharp
using nanoFramework.Hardware.ESP32;
    
    // Pin Definitions for the Cheap Yellow Display
    const int XPT2046_CS = Gpio.IO33;      // Chip Select
    const int XPT2046_PenIRQ = Gpio.IO36;  // Touch detected interupt
    const int XPT2046_COPI = Gpio.IO32;
    const int XPT2046_CIPO = Gpio.IO39;
    const int XPT2046_CLK = Gpio.IO25;

    // If you're using an ESP32, use nanoFramework.Hardware.Esp32 to remap the SPI pins
    Configuration.SetPinFunction(XPT2046_COPI, DeviceFunction.SPI1_MOSI);
    Configuration.SetPinFunction(XPT2046_CIPO, DeviceFunction.SPI1_CLOCK);
    Configuration.SetPinFunction(XPT2046_CLK, DeviceFunction.SPI1_MISO);
```

```csharp
using System.Device.Spi;

SpiDevice spiDevice;
SpiConnectionSettings connectionSettings;
connectionSettings = new SpiConnectionSettings(1, XPT2046_CS);
connectionSettings.ClockFrequency = 2_000_000;
connectionSettings.DataBitLength = 8;
connectionSettings.DataFlow = DataFlow.MsbFirst;
connectionSettings.Mode = SpiMode.Mode0;

// Then you create your SPI device by passing your settings
spiDevice = SpiDevice.Create(connectionSettings);

using GpioController gpio = new();
using Xpt2046 sensor = new(spiDevice);
var ver = sensor.GetVersion();

Debug.WriteLine($"version: {ver}");

var point = sensor.GetPoint();

Debug.WriteLine($"ID: {point.TouchId}, X: {point.X}, Y: {point.Y}, Weight: {point.Weigth}, Misc: {point.Miscelaneous}");

```

Use with an interupt pin. Note that interupts that happen during conversion are not reliable. Hence setting touchDetected after calling GetPoint();

```csharp
bool touchDetected = false;

gpio.OpenPin(XPT2046_PenIRQ, PinMode.Input);
// This will enable an event on GPIO36 on falling edge when the screen if touched
gpio.RegisterCallbackForPinValueChangedEvent(XPT2046_PenIRQ, PinEventTypes.Falling, TouchInterrupCallback);

while (true)
{
    if (touchDetected) {
        var point = sensor.GetPoint();
        touchDetected = false;
        
        Debug.WriteLine($"ID: {point.TouchId}, X: {point.X}, Y: {point.Y}, Weight: {point.Weigth}, Misc: {point.Miscelaneous}");

        Thread.Sleep(500);

    }

    Thread.Sleep(20);
}

void TouchInterrupCallback(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
{
    touchDetected = true;
}
```
