# XPT2046 - Touch screen controller

The XPT2046 from XPTEK is a 4-wire resistive touch screen controller that incorporates a 12-bit 125 kHz successive approximation register type A/D converter. The XPT2046 can detect the pressed screen location by performing two A/D conversions. As well as the location, the XPT2046 also measures touch screen pressure. A multiplexer allows it to measure chip temperature and battery voltage.

The XPT2046 is common in a number of touch screen modules from suppliers like WaveShare and an number of ESP32 based display boards.

Communication is via the SPI bus which can communicate at maximum speed of 2Mhz and uses Mode 0 for the clock polarity/phase setting. The host sends an 8 bit command and the chip responds with 16 bits of data althought the last few bits are always zero.

An additional interupt pin which is usually in a high state and goes low when a touch is detected.

The chip can use an internal voltage reference or be configured to use an external reference if more accuracy is required.

The XPT2046 will enter power down mode and stop communicating when the chip select pin is taken high.

## Documentation

Datasheet - https://www.waveshare.com/wiki/File:XPT2046-EN.pdf

## Usage

**Important**: make sure you properly setup the SPI pins especially for ESP32 before creating the `SPIBus`, make sure you install the `nanoFramework.Hardware.ESP32 nuget`:

```csharp
using nanoFramework.Hardware.ESP32;
    
    // Pin Definitions for the Cheap Yellow Display
    private const int XPT2046_CS = 33;      // Chip Select
    private const int XPT2046_PenIRQ = 35;  // Touch detected interupt
    private const int XPT2046_COPI = 32;
    private const int XPT2046_CIPO = 39;
    private const int XPT2046_CLK = 25;

    // If you're using an ESP32, use nanoFramework.Hardware.Esp32 to remap the SPI pins
    Configuration.SetPinFunction(XPT2046_COPI, DeviceFunction.SPI1_MOSI);
    Configuration.SetPinFunction(XPT2046_CIPO, DeviceFunction.SPI1_CLOCK);
    Configuration.SetPinFunction(XPT2046_CLK, DeviceFunction.SPI1_MISO);
```

```csharp
using System.Device.Spi;

SpiDevice spiDevice;
SpiConnectionSettings connectionSettings;
SpiBusInfo spiBusInfo = SpiDevice.GetBusInfo(1);
connectionSettings = new SpiConnectionSettings(1, XPT2046_CS);
connectionSettings.ClockFrequency = 1_000_000;
connectionSettings.DataBitLength = 8;
connectionSettings.DataFlow = DataFlow.MsbFirst;
connectionSettings.Mode = SpiMode.Mode0;

// Then you create your SPI device by passing your settings
spiDevice = SpiDevice.Create(connectionSettings);

using GpioController gpio = new();
using XPT2046 sensor = new(spiDevice);
var ver = sensor.GetVersion();
Debug.WriteLine($"version: {ver}");
```

```csharp
bool touchDetected = false;

gpio.OpenPin(XPT2046_PenIRQ, PinMode.Input);
// This will enable an event on GPIO36 on falling edge when the screen if touched
gpio.RegisterCallbackForPinValueChangedEvent(XPT2046_PenIRQ, PinEventTypes.Falling, TouchInterrupCallback);

while (true)
{
    if (touchDetected) {
        var point = sensor.GetPoint();
        Debug.WriteLine($"ID: {point.TouchId}, X: {point.X}, Y: {point.Y}, Weight: {point.Weigth}, Misc: {point.Miscelaneous}");

        Thread.Sleep(500);
        touchDetected = false;
    }

    Thread.Sleep(20);
}

void TouchInterrupCallback(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
{
    touchDetected = true;
}
```
