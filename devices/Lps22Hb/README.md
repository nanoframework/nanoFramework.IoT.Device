# LPS22HB - MEMS nano pressure sensor: 260-1260 hPa absolute digital output barometer

Some of the applications mentioned by the datasheet:

- Altimeters and barometers for portable devices
- GPS applications
- Weather station equipment
- Sport watches

## Documentation

- [Datasheet](https://www.st.com/resource/en/datasheet/lps22hb.pdf)

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
using Iot.Device.Lps22Hb;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

using Lps22Hb lps22HdDevice = new(CreateI2cDevice(), FifoMode.Bypass);

while (true)
{
    var tempValue = lps22HdDevice.Temperature;
    var pressure = lps22HdDevice.Pressure;

    Debug.WriteLine($"Temperature: {tempValue.DegreesCelsius:F1}\u00B0C");
    Debug.WriteLine($"Pressure: {pressure.Hectopascals:F1}hPa");

    Thread.Sleep(1000);
}

I2cDevice CreateI2cDevice()
{
    I2cConnectionSettings settings = new(1, Lps22Hb.I2cAddress);
    return I2cDevice.Create(settings);
}

I2cDevice CreateI2cDevice()
{
    I2cConnectionSettings settings = new(1, Lps22Hb.DefaultI2cAddress);
    return I2cDevice.Create(settings);
}

```
