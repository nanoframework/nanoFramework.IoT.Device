# AM2320 - Temperature and Humidity sensor

AM2320 is a temperature and humidity sensor, sensible to 0.1 degree and 0.1 relative humidity.

## Documentation

- [AM2320](https://cdn-shop.adafruit.com/product-files/3721/AM2320.pdf)

## Usage

**Important**: make sure you properly setup the I2C pins especially for ESP32 before creating the `I2cDevice`, make sure you install the `nanoFramework.Hardware.ESP32 nuget`:

```csharp
//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
```

For other devices like STM32, please make sure you're using the pre-set pins for the SPI bus you want to use. The chip select can as well be pre setup.

Here is an example how to use the APA102:

```csharp
using System;
using System.Device.Spi;
using System.Drawing;
using System.Threading;
using Iot.Device.Apa102;

var random = new Random();

using SpiDevice spiDevice = SpiDevice.Create(new SpiConnectionSettings(1, 42)
{
    ClockFrequency = 20_000_000,
    DataFlow = DataFlow.MsbFirst,
    Mode = SpiMode.Mode0 // ensure data is ready at clock rising edge
});
using Apa102 apa102 = new Apa102(spiDevice, 16);

while (true)
{
    for (var i = 0; i < apa102.Pixels.Length; i++)
    {
        apa102.Pixels[i] = Color.FromArgb(255, random.Next(256), random.Next(256), random.Next(256));
    }

    apa102.Flush();
    Thread.Sleep(1000);
}
```
