// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Device.I2c;
using Iot.Device.Common;
using Iot.Device.Lps25h;
using UnitsNet;
using System.Diagnostics;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
//Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

// I2C address on SenseHat board
const int I2cAddress = 0x5c;

// set this to the current sea level pressure in the area for correct altitude readings
var defaultSeaLevelPressure = WeatherHelper.MeanSeaLevel;

using Lps25h th = new(CreateI2cDevice());
while (true)
{
    Temperature tempValue = th.Temperature;
    Pressure preValue = th.Pressure;
    Length altValue = WeatherHelper.CalculateAltitude(preValue, defaultSeaLevelPressure, tempValue);

    Debug.WriteLine($"Temperature: {tempValue.DegreesCelsius:0.#}\u00B0C");
    Debug.WriteLine($"Pressure: {preValue.Hectopascals:0.##}hPa");
    Debug.WriteLine($"Altitude: {altValue:0.##}m");
    Thread.Sleep(1000);
}

I2cDevice CreateI2cDevice()
{
    I2cConnectionSettings settings = new(1, I2cAddress);
    return I2cDevice.Create(settings);
}
