# BMxx80 Device Family

BMxx80 is a device family that senses temperature, barometric pressure, altitude, humidity and VOC gas.

SPI and I2C can be used to communicate with the device (only I2C implemented so far).

## Documentation

The implementation supports the following devices:

- BMP280 temperature and barometric pressure sensor ([Datasheet](https://cdn-shop.adafruit.com/datasheets/BST-BMP280-DS001-11.pdf))
- BME280 temperature, barometric pressure and humidity sensor ([Datasheet](https://ae-bst.resource.bosch.com/media/_tech/media/datasheets/BST-BME280-DS002.pdf))
- BME680 temperature, barometric pressure, humidity and VOC gas sensor ([Datasheet](https://ae-bst.resource.bosch.com/media/_tech/media/datasheets/BST-BME680-DS001.pdf))

## Usage

### BME280

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
// bus id on the MCU
const int busId = 1;
I2cConnectionSettings i2cSettings = new(busId, Bme280.DefaultI2cAddress);
using I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);
using Bme280 bme80 = new Bme280(i2cDevice)
{
    // set higher sampling
    TemperatureSampling = Sampling.LowPower,
    PressureSampling = Sampling.UltraHighResolution,
    HumiditySampling = Sampling.Standard,

};

// Perform a synchronous measurement
var readResult = bme80.Read();

// Note that if you already have the pressure value and the temperature, you could also calculate altitude by using
// var altValue = WeatherHelper.CalculateAltitude(preValue, defaultSeaLevelPressure, tempValue) which would be more performant.
bme80.TryReadAltitude(defaultSeaLevelPressure, out var altValue);

Debug.WriteLine($"Temperature: {readResult.Temperature?.DegreesCelsius:0.#}\u00B0C");
Debug.WriteLine($"Pressure: {readResult.Pressure?.Hectopascals:0.##}hPa");
Debug.WriteLine($"Altitude: {altValue.Meters:0.##}m");
Debug.WriteLine($"Relative humidity: {readResult.Humidity?.Percent:0.#}%");
```

### BMP680

```csharp
// The I2C bus ID on the MCU
const int busId = 1;

I2cConnectionSettings i2cSettings = new(busId, Bme680.DefaultI2cAddress);
I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);

using Bme680 bme680 = new Bme680(i2cDevice, Temperature.FromDegreesCelsius(20.0));

// reset will change settings back to default
bme680.Reset();

// Perform a synchronous measurement
var readResult = bme680.Read();

// Print out the measured data
Debug.WriteLine($"Gas resistance: {readResult.GasResistance?.Ohms:0.##}Ohm");
Debug.WriteLine($"Temperature: {readResult.Temperature?.DegreesCelsius:0.#}\u00B0C");
Debug.WriteLine($"Pressure: {readResult.Pressure?.Hectopascals:0.##}hPa");
Debug.WriteLine($"Relative humidity: {readResult.Humidity?.Percent:0.#}%");
```

### BMP280

```csharp
// bus id on the MCU
const int busId = 1;

I2cConnectionSettings i2cSettings = new(busId, Bmp280.DefaultI2cAddress);
I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);
using var i2CBmp280 = new Bmp280(i2cDevice);

// set higher sampling
i2CBmp280.TemperatureSampling = Sampling.LowPower;
i2CBmp280.PressureSampling = Sampling.UltraHighResolution;

// Perform a synchronous measurement
var readResult = i2CBmp280.Read();

// Print out the measured data
Debug.WriteLine($"Temperature: {readResult.Temperature?.DegreesCelsius:0.#}\u00B0C");
Debug.WriteLine($"Pressure: {readResult.Pressure?.Hectopascals:0.##}hPa");
```

You also have 3 examples on how to use this device binding are available in the [samples](samples) folder.

The following fritzing diagram illustrates one way to wire up the BMP280 with an MCU like ESP32 using I2C:

![ESP32 Breadboard diagram](./rpi-bmp280_i2c.png)

General:

| Bmp280 | MCU |
|--------|:---------:|
|Vin| Power pin|
|GND| Ground|

I2C:

| Bmp280 | MCU |
|--------|:---------:|
|SCK| I2C clock pin|
|SDI| I2C data pin|

### Connection Type

The following connection types are supported by this binding.

- [X] I2C
- [ ] SPI
