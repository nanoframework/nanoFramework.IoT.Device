// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Common;
using Iot.Device.Shtc3;
using UnitsNet;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
//Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new(1, Iot.Device.Shtc3.Shtc3.DefaultI2cAddress);
using I2cDevice device = I2cDevice.Create(settings);
using Shtc3 sensor = new(device);
Debug.WriteLine($"Sensor Id: {sensor.Id}");
while (true)
{
    // Try sensor measurement in normal power mode
    if (sensor.TryGetTemperatureAndHumidity(out var temperature, out var relativeHumidity))
    {
        Debug.WriteLine($"====================In normal power mode===========================");
        ConsoleWriteInfo(temperature, relativeHumidity);
    }

    // Try sensor measurement in low power mode
    if (sensor.TryGetTemperatureAndHumidity(out temperature, out relativeHumidity, lowPower: true))
    {
        Debug.WriteLine($"====================In low power mode===========================");
        ConsoleWriteInfo(temperature, relativeHumidity);
    }

    // Set sensor in sleep mode
    sensor.Sleep();

    Debug.WriteLine("");
    Thread.Sleep(1000);
}

void ConsoleWriteInfo(Temperature temperature, RelativeHumidity relativeHumidity)
{
    Debug.WriteLine($"Temperature: {temperature.DegreesCelsius:0.#}\u00B0C");
    Debug.WriteLine($"Humidity: {relativeHumidity.Percent:0.#}%");
    // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
    Debug.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(temperature, relativeHumidity).DegreesCelsius:0.#}\u00B0C");
    Debug.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(temperature, relativeHumidity).DegreesCelsius:0.#}\u00B0C");
}
