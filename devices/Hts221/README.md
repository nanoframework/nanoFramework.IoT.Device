# HTS221 - Capacitive digital sensor for relative humidity and temperature

Some of the applications mentioned by the datasheet:

- Air conditioning, heating and ventilation
- Air humidifiers
- Refrigerators
- Wearable devices
- Smart home automation
- Industrial automation
- Respiratory equipment
- Asset and goods tracking

## Documentation

- [Datasheet](https://www.st.com/resource/en/datasheet/hts221.pdf)

## Usage

**Important**: make sure you properly setup the I2C pins especially for ESP32 before creating the `I2cDevice`, make sure you install the `nanoFramework.Hardware.ESP32 nuget`:

```csharp
//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
```

For other devices like STM32, please make sure you're using the preset pins for the I2C bus you want to use.

```csharp
using System;
using System.Threading;
using System.Device.I2c;
using Iot.Device.Common;
using Iot.Device.Hts221;
using UnitsNet;

// I2C address on SenseHat board
const int I2cAddress = 0x5F;

using Hts221 th = new(CreateI2cDevice());
while (true)
{
    var tempValue = th.Temperature;
    var humValue = th.Humidity;

    Debug.WriteLine($"Temperature: {tempValue.DegreesCelsius:0.#}\u00B0C");
    Debug.WriteLine($"Relative humidity: {humValue:0.#}%");

    // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
    Debug.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(tempValue, humValue).DegreesCelsius:0.#}\u00B0C");
    Debug.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(tempValue, humValue).DegreesCelsius:0.#}\u00B0C");
    Thread.Sleep(1000);
}

I2cDevice CreateI2cDevice()
{
    I2cConnectionSettings settings = new(1, I2cAddress);
    return I2cDevice.Create(settings);
}

```
