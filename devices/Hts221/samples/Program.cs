// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Common;
using Iot.Device.Hts221;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
//Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

using Hts221 th = new(CreateI2cDevice());
while (true)
{
    var tempValue = th.Temperature;
    var humValue = th.Humidity;

    Debug.WriteLine($"Temperature: {tempValue.DegreesCelsius:F1}\u00B0C");
    Debug.WriteLine($"Relative humidity: {humValue.Percent:F1}%");

    // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
    Debug.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(tempValue, humValue).DegreesCelsius:F1}\u00B0C");
    Debug.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(tempValue, humValue).DegreesCelsius:F1}\u00B0C");

    Thread.Sleep(1000);
}

I2cDevice CreateI2cDevice()
{
    I2cConnectionSettings settings = new(1, Hts221.DefaultI2cAddress);
    return I2cDevice.Create(settings);
}
