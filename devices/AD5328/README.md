# AD5328 - Digital to Analog Convertor

AD5328 is an Digital-to-Analog converter (DAC) with 12 bits of resolution.

## Documentation

Product information and documentation can he found [here](https://www.analog.com/en/products/ad5328.html)

## Usage

**Important**: make sure you properly setup the SPI pins especially for ESP32 before creating the `SpiDevice`, make sure you install the `nanoFramework.Hardware.ESP32 nuget`:

```csharp
//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the SPI GPIOs
// used for the bus
Configuration.SetPinFunction(21, DeviceFunction.SPI1_MOSI);
Configuration.SetPinFunction(22, DeviceFunction.SPI1_MISO);
Configuration.SetPinFunction(23, DeviceFunction.SPI1_CLOCK);
// Make sure as well you are using the right chip select
```

For other devices like STM32, please make sure you're using the preset pins for the SPI bus you want to use. The chip select can as well be pre setup.

```csharp
using System.Device.Spi;
using System.Threading;
using Iot.Device.DAC;
using UnitsNet;

var spisettings = new SpiConnectionSettings(1, 42)
{
    Mode = SpiMode.Mode2
};

var spidev = SpiDevice.Create(spisettings);
var dac = new AD5328(spidev, ElectricPotential.FromVolts(2.5), ElectricPotential.FromVolts(2.5));
Thread.Sleep(1000);
dac.SetVoltage(0, ElectricPotential.FromVolts(1));
```
