# ADXL343 - Accelerometer

ADXL343 is a small, thin, low power, 3-axis accelerometer with high resolution (13-bit) measurement at up to ±16g.

## Documentation

In [English](https://www.analog.com/media/en/technical-documentation/data-sheets/adxl343.pdf)

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

Here is the basic usage:

```csharp
var i2c = new(new I2cConnectionSettings(I2cBusId, I2cAddr));

Adxl343 sensor = new Adxl343(i2c, GravityRange.Range16);

Debug.WriteLine("Testing Vector...");

Vector3 v = new Vector3();

while (true)
{
    if (sensor.TryGetAcceleration(ref v))
    {
        Debug.WriteLine("Get Vector Successful");
        Debug.WriteLine($"X = 0x{v.X}, Y = 0x{v.Y}, Z = 0x{v.Z}");
    }
    Thread.Sleep(1000);
}
```

Also you'll find a lot of functions which will allow you to have a very detailed and precise way to setup the sensor.
