# SCD4x - Temperature & Humidity & CO2 Sensor

The SCD4x is Sensirion's next generation miniature CO2 sensor. This sensor builds on the photoacoustic sensing principle and Sensirion's patented PAsens(r) and CMOSens(r) technology to offer high accuracy at an unmatched price and smallest form factor.

## Documentation

- SCD4X [datasheet](https://cdn.sparkfun.com/assets/d/4/9/a/d/Sensirion_CO2_Sensors_SCD4x_Datasheet.pdf)

## Usage

### Hardware Required

- SCD4X
- Male/Female Jumper Wires

### Circuit

- SCL - SCL
- SDA - SDA
- VCC - 3.3V
- GND - GND

### Code

**Important**: make sure you properly setup the I2C pins especially for ESP32 before creating the `I2cDevice`, make sure you install the `nanoFramework.Hardware.ESP32 nuget`:

```csharp
//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
Configuration.SetPinFunction(23, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
```

For other devices like STM32, please make sure you're using the preset pins for the I2C bus you want to use.

```csharp
I2cConnectionSettings settings = new(1, Scd4x.I2cDefaultAddress);
using I2cDevice device = I2cDevice.Create(settings);
using Scd4x sensor = new(device);
sensor.StopPeriodicMeasurement();
var serialNumber = sensor.GetSerialNumber();
Console.WriteLine($"Serial number: {serialNumber}");
var offset = sensor.GetTemperatureOffset();
Console.WriteLine($"Temperature offset: {offset.DegreesCelsius}");
sensor.SetTemperatureOffset(Temperature.FromDegreesCelsius(4));
offset = sensor.GetTemperatureOffset();
Console.WriteLine($"New temperature offset: {offset.DegreesCelsius}");

sensor.StartPeriodicMeasurement();
while (true)
{
    if (!sensor.IsDataReady())
    {
        Thread.Sleep(1000);
        continue;
    }

    var data = sensor.ReadData();
    Console.WriteLine($"Temperature: {data.Temperature.DegreesCelsius} \u00B0C");
    Console.WriteLine($"Relative humidity: {data.RelativeHumidity.Percent} %RH");
    Console.WriteLine($"CO2: {data.CO2} PPM");
}

```