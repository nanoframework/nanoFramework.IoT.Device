# Sht4x/SHT40/SHT41/SHT45 - Temperature & Humidity Sensor with internal heater
Sht4x is the next generation of Sensirion's temperature and humidity sensors. This project supports SHT40, SHT41, SHT43 and SHT45.

## Documentation

- SHT4X [datasheet](https://sensirion.com/media/documents/33FD6951/662A593A/HT_DS_Datasheet_SHT4x.pdf)

## Usage

### Hardware Required

- Sht4x
- Male/Female Jumper Wires

### Circuit

- SCL - SCL
- SDA - SDA
- VCC - 5V
- GND - GND

### Code

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
I2cConnectionSettings settings = new I2cConnectionSettings(1, (byte)I2cAddress.AddrLow);
I2cDevice device = I2cDevice.Create(settings);

using Sht4X sensor = new(device);
var data = sensor.ReadData(MeasurementMode.NoHeaterHighPrecision);
Debug.WriteLine($"Temperature: {data.Temperature.DegreesCelsius:0.#}\u00B0C");
Debug.WriteLine($"Relative humidity: {data.RelativeHumidity.Percent:0.#}%RH");
}
```