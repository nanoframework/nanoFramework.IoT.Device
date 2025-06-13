# XPT2046 - Touch screen controller

The XPT2046 from XPTEK is a 4-wire resistive touch screen controller that incorporates a 12-bit 125 kHz successive approximation register type A/D converter. The XPT2046 can detect the pressed screen location by performing two A/D conversions. As well as the location, the XPT2046 also measures touch screen pressure. A multiplexer allows it to measure chip temperature and battery voltage.

The XPT2046 is common in a number of touch screen modules from suppliers like WaveShare and an number of ESP32 based display boards.

Communication is via the SPI bus and an additional interupt pin which is usually in a high state and goes low when a touch is detected.

The chip can use an internal voltage reference or be configured to use an external reference if more accuracy is required.

The XPT2046 will enter power down mode and stop communicating when the chip select pin is taken high.

## Documentation

Datasheet - https://www.waveshare.com/wiki/File:XPT2046-EN.pdf

## Usage

**Important**: make sure you properly setup the SPI pins especially for ESP32 before creating the `SPIBus`, make sure you install the `nanoFramework.Hardware.ESP32 nuget`:

```csharp
    // Pin Definitions for the Cheap Yellow Display
    private const int XPT2046_CS = 33; // Chip Select
    private const int XPT2046_PenIRQ = 35;
    private const int XPT2046_MOSI = 32;
    private const int XPT2046_MISO = 39;
    private const int XPT2046_CLK = 25;

    // If you're using an ESP32, use nanoFramework.Hardware.Esp32 to remap the SPI pins
    Configuration.SetPinFunction(32, DeviceFunction.SPI1_MOSI);
    Configuration.SetPinFunction(25, DeviceFunction.SPI1_CLOCK);
    Configuration.SetPinFunction(39, DeviceFunction.SPI1_MISO);
```

```csharp

using System.Device.Spi;

SpiDevice spiDevice;
SpiConnectionSettings connectionSettings;

SpiBusInfo spiBusInfo = SpiDevice.GetBusInfo(1);

connectionSettings = new SpiConnectionSettings(1, XPT2046_CS);
//TODO: Check these against the XP2046 requirements
connectionSettings.ClockFrequency = 1_000_000;
connectionSettings.DataBitLength = 8;
connectionSettings.DataFlow = DataFlow.LsbFirst;
connectionSettings.Mode = SpiMode.Mode2;

// Then you create your SPI device by passing your settings
spiDevice = SpiDevice.Create(connectionSettings);

using GpioController gpio = new();

using XPT2046 sensor = new(spiDevice);
var ver = sensor.GetVersion();
Debug.WriteLine($"version: {ver}");

//TODO: Update this example does the driver use interupts internally or not?
sensor.SetInterruptMode(false);
Debug.WriteLine($"Period active: {sensor.PeriodActive}");
Debug.WriteLine($"Period active in monitor mode: {sensor.MonitorModePeriodActive}");
Debug.WriteLine($"Time to enter monitor: {sensor.MonitorModeDelaySeconds} seconds");
Debug.WriteLine($"Monitor mode: {sensor.MonitorModeEnabled}");
Debug.WriteLine($"Proximity sensing: {sensor.ProximitySensingEnabled}");

gpio.OpenPin(XPT2046_PenIRQ, PinMode.Input);
// This will enable an event on GPIO36 on falling edge when the screen if touched
gpio.RegisterCallbackForPinValueChangedEvent(XPT2046_PenIRQ, PinEventTypes.Falling, TouchInterrupCallback);

while (true)
{
    Thread.Sleep(20);
}

void TouchInterrupCallback(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
{
    //TODO: Move this to simply toggle a variable which we check in the main loop

    Debug.WriteLine("Touch interrupt");
    var points = sensor.GetNumberPoints();
    if (points == 1)
    {
        var point = sensor.GetPoint(true);
                // Some controllers supports as well events, you can get access to them as well thru point.Event
        Debug.WriteLine($"ID: {point.TouchId}, X: {point.X}, Y: {point.Y}, Weight: {point.Weigth}, Misc: {point.Miscelaneous}");
    }
    else if (points == 2)
    {
        var dp = sensor.GetDoublePoints();
        Debug.WriteLine($"ID: {dp.Point1.TouchId}, X: {dp.Point1.X}, Y: {dp.Point1.Y}, Weight: {dp.Point1.Weigth}, Misc: {dp.Point1.Miscelaneous}");
        Debug.WriteLine($"ID: {dp.Point2.TouchId}, X: {dp.Point2.X}, Y: {dp.Point2.Y}, Weight: {dp.Point2.Weigth}, Misc: {dp.Point2.Miscelaneous}");
    }
}
```
