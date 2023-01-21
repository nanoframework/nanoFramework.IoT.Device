// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Common;
using Iot.Device.Sht4x;
using UnitsNet;

I2cConnectionSettings settings = new(1, Sht4x.DefaultI2cAddress);
using I2cDevice device = I2cDevice.Create(settings);
using Sht4x sensor = new(device);

// Async loop.
for (int i = 0; i < 3; ++i)
{
    (RelativeHumidity? hum, Temperature? temp) = await sensor.ReadHumidityAndTemperatureAsync();

    Debug.WriteLine(temp is not null
        ? $"Temperature: {temp.Value}"
        : "Temperature: CRC check failed.");

    Debug.WriteLine(hum is not null
        ? $"Relative humidity: {hum.Value}"
        : "Relative humidity: CRC check failed.");

    if (temp is not null && hum is not null)
    {
        // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
        Debug.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(temp.Value, hum.Value)}");
        Debug.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(temp.Value, hum.Value)}");
    }

    Debug.WriteLine("");

    await Task.Delay(1000);
}

// Property-based access.
for (int i = 0; i < 3; ++i)
{
    Debug.WriteLine($"Temperature: {sensor.Temperature}");
    Debug.WriteLine($"Relative humidity: {sensor.RelativeHumidity}");

    Debug.WriteLine("");

    Thread.Sleep(1000);
}
