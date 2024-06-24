// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Common;
using Iot.Device.Sht4x;
using nanoFramework.Hardware.Esp32;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
Configuration.SetPinFunction(8, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(9, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new(1, Sht4X.DefaultI2CAddress);
using I2cDevice device = I2cDevice.Create(settings);
using Sht4X sensor = new(device);
while (true)
{
    var data = sensor.ReadData(MeasurementMode.NoHeaterHighPrecision);

    Debug.WriteLine($"Temperature: {data.Temperature.DegreesCelsius}\u00B0C");
    Debug.WriteLine($"Relative humidity: {data.RelativeHumidity.Percent}%RH");

    // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
    Debug.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(data.Temperature, data.RelativeHumidity).DegreesCelsius}\u00B0C");
    Debug.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(data.Temperature, data.RelativeHumidity).DegreesCelsius}\u00B0C");
    Debug.WriteLine("");

    Thread.Sleep(1000);
}
