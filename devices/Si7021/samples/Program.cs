// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Common;
using Iot.Device.Si7021;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
//Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

//////////////////////////////////////////////////////////////////////
// For Silabs SLSTK3701A Giant Gecko the Si7021 is connected to I2C3

I2cConnectionSettings settings = new(3, Si7021.DefaultI2cAddress);
using I2cDevice device = I2cDevice.Create(settings);
using Si7021 sensor = new(device, Resolution.Resolution1);

Debug.WriteLine("");
Debug.WriteLine($"Si702 SN {sensor.SerialNumber.AsText()} fw v{sensor.Revision}");
Debug.WriteLine("");

while (true)
{
    var tempValue = sensor.Temperature;
    var humValue = sensor.Humidity;

    Debug.WriteLine($"Temperature: {tempValue.DegreesCelsius:N2}\u00B0C");
    Debug.WriteLine($"Relative humidity: {humValue.Percent:N2}%");

    // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
    Debug.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(tempValue, humValue).DegreesCelsius:N2}\u00B0C");
    Debug.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(tempValue, humValue).DegreesCelsius:N2}\u00B0C");
    Debug.WriteLine("");

    Thread.Sleep(1000);
}
