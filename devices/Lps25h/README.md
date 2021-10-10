# LPS25H - Piezoresistive pressure and thermometer sensor

Some of the applications mentioned by the datasheet:

- Altimeter and barometer for portable devices
- GPS applications
- Weather station equipment
- Sport watches

## Documentation

- You can find the datasheet [here](https://www.st.com/resource/en/datasheet/lps25h.pdf)

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
class Program
{
    // I2C address on SenseHat board
    public const int I2cAddress = 0x5c;

    static void Main(string[] args)
    {
        using (var th = new Lps25h(CreateI2cDevice()))
        {
            while (true)
            {
                var tempValue = th.Temperature;
                var preValue = th.Pressure;
                var altValue = WeatherHelper.CalculateAltitude(preValue, defaultSeaLevelPressure, tempValue);

                Debug.WriteLine($"Temperature: {tempValue.Celsius:0.#}\u00B0C");
                Debug.WriteLine($"Pressure: {preValue.Hectopascal:0.##}hPa");
                Debug.WriteLine($"Altitude: {altValue:0.##}m");
                Thread.Sleep(1000);
            }
        }
    }

    private static I2cDevice CreateI2cDevice()
    {
        var settings = new I2cConnectionSettings(1, I2cAddress);
        return I2cDevice.Create(settings);
    }
}
```
