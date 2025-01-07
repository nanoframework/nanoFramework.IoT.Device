# Vl6180X - distance sensor

The Vl6180X sensor is a Time-to-Flight sensor measuring precisely distances. The sensor allows you to get precise short distance measurement (from 5 millimeters to 2 meters) as well as long distance measurement (up to 8 meters but with a decreased precision). This sensor is a laser ranging sensor. It is using laser pulses to measure the distances.

## Documentation

**Vl6180X** [datasheet](https://www.st.com/resource/en/datasheet/vl6180x.pdf)


## Usage

**Important**: make sure you properly setup the I2C pins especially for ESP32 before creating the `I2cDevice`, make sure you install the `nanoFramework.Hardware.ESP32 nuget`:

```csharp
// when connecting to an ESP32 device, need to configure the I2C GPIOs used for the bus
Configuration.SetPinFunction(11, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(10, DeviceFunction.I2C1_CLOCK);

using VL6180X sensor = new(I2cDevice.Create(new I2cConnectionSettings(1, VL6180X.DefaultI2cAddress)));
sensor.Init();
while (true)
{
    var distance = sensor.ReadRange();
    Console.WriteLine($"Distance: {distance.Centimeters} cm.");
    Thread.Sleep(500);
}
```