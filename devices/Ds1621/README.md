# Ds1621 - 1-Wire Digital Thermometer with Programmable Resolution

The Ds1621 digital thermometer provides 9-bit to 12-bit temperature measurements in Celsius and has an alarm function with nonvolatile user-programmable upper and lower trigger points.

## Documentation

[Datasheet](https://datasheets.maximintegrated.com/en/ds/Ds1621.pdf)

Original code was written for ESP32

## Usage

**Important**: Make sure you properly setup the I2C pins for ESP32 before creating the `I2cDevice`. For this, make sure you install the `nanoFramework.Hardware.Esp32` NuGet and use the `Configuration` class to configure the pins:

```csharp
// When connecting to an ESP32 device you will need to configure the I2C GPIOs used for the bus.
Configuration.SetPinFunction(Gpio.IO21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(Gpio.IO22, DeviceFunction.I2C1_CLOCK);
```

For other devices like STM32, please make sure you're using the preset pins for the I2C bus you want to use.

The following example demonstrates using the temperature and alarm functions of the Ds1621.

```csharp
using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Ds1621;
using nanoFramework.Hardware.Esp32;
using UnitsNet;

string alarmState;

// Setup ESP32 I2C port.
Configuration.SetPinFunction(Gpio.IO21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(Gpio.IO22, DeviceFunction.I2C1_CLOCK);

// Setup Ds1621 device.
I2cConnectionSettings i2cSettings = new I2cConnectionSettings(1, Ds1621.DefaultI2cAddress);
I2cDevice i2cDevice = new I2cDevice(i2cSettings);

Ds1621 thermometer = new Ds1621(i2cDevice, MeasurementMode.Single);

// Set temperature alarms.
thermometer.LowTemperatureAlarm = Temperature.FromDegreesFahrenheit(65);
thermometer.HighTemperatureAlarm = Temperature.FromDegreesFahrenheit(80);

while (true)
{
    // Start temperature conversion.
    thermometer.MeasureTemperature();

    // Wait for temperature conversion to complete.
    while (thermometer.IsMeasuringTemperature)
    {
        Thread.Sleep(10);
    }

    Temperature temperature = thermometer.GetTemperature();

    // Check temperature alarm states.
    if (thermometer.HasLowTemperatureAlarm)
    {
        alarmState = "[Low Temperature]";
    }
    else if (thermometer.HasHighTemperatureAlarm)
    {
        alarmState = "[High Temperature]";
    }
    else
    {
        alarmState = string.Empty;
    }

    Debug.WriteLine($"{DateTime.UtcNow} : {temperature.DegreesCelsius:F1}°C / {temperature.DegreesFahrenheit:F1}°F {alarmState}");

    Thread.Sleep(1000);
}
```
