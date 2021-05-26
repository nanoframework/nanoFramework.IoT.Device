# SHTC3 - Temperature & Humidity Sensor

SHTC3 is a digital humidity and temperature sensor designed especially for battery-driven high-volume consumer electronics application.
To reduce power cosumption this project use capability of sensor to allow measurement in low power mode and active sleep mode.

## Usage

```C#
I2cConnectionSettings settings = new I2cConnectionSettings(1, Iot.Device.Shtc3.Shtc3.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);

using (Shtc3 sensor = new Shtc3(device))
{
    if (sensor.TryGetTemperatureAndHumidity(out var temperature, out var relativeHumidity))
    {
        // temperature (℃)
        Console.WriteLine($"Temperature: {temperature.DegreesCelsius:0.#}\u00B0C");
        // humidity (%)
        Console.WriteLine($"Humidity: {relativeHumidity.Percent:0.#}%");
    }

    // Make sensor in sleep mode
    sensor.Sleep();
}
```

## References
https://www.sensirion.com/fileadmin/user_upload/customers/sensirion/Dokumente/2_Humidity_Sensors/Datasheets/Sensirion_Humidity_Sensors_SHTC3_Datasheet.pdf
