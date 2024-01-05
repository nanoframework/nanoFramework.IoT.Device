# MAX1704x/MAX17043/MAX17044/MAX17048/MAX17049 - Battery gauge
Library designed for controlling MAX1704x IC.

## Functions

The binding supports all functionalities for MAX1704x family. Implemented based on [SparkFun implementation](https://github.com/sparkfun/SparkFun_MAX1704x_Fuel_Gauge_Arduino_Library/blob/main/src/SparkFun_MAX1704x_Fuel_Gauge_Arduino_Library.cpp).

## Classes

* Max17043 - class for MAX17043
* Max17044 - class for MAX17044
* Max17048 - class for MAX17048
* Max17049 - class for MAX17049
* Max1704x - base class for all max IC

## Usage

```csharp
// Make sure your pins are configured correctly especially for ESp32
var i2cDevice = I2cDevice.Create(new I2cConnectionSettings(1, Max1704X.DefaultAddress));
var max = new Max17048(i2cDevice);
Console.WriteLine($"Voltage: {max.BatteryVoltage.Volts} V");
Console.WriteLine($"Percent: {max.BatteryPercent.Percent} %");
```

**Important**: make sure you properly setup the I2C pins especially for ESP32 before creating the `I2cDevice`.