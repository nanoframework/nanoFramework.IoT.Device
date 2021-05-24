﻿# LPS25H - Piezoresistive pressure and thermometer sensor

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

                Console.WriteLine($"Temperature: {tempValue.Celsius:0.#}\u00B0C");
                Console.WriteLine($"Pressure: {preValue.Hectopascal:0.##}hPa");
                Console.WriteLine($"Altitude: {altValue:0.##}m");
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
