// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Bmp180;
using Iot.Device.Common;
using UnitsNet;

Debug.WriteLine("Hello Bmp180!");

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
//Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

// bus id on the MCU
const int busId = 1;

I2cConnectionSettings i2cSettings = new(busId, Bmp180.DefaultI2cAddress);
using I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);

using Bmp180 i2cBmp280 = new(i2cDevice);
// set samplings
i2cBmp280.SetSampling(Sampling.Standard);

// read values
Temperature tempValue = i2cBmp280.ReadTemperature();
Debug.WriteLine($"Temperature: {tempValue.DegreesCelsius:0.#}\u00B0C");
Pressure preValue = i2cBmp280.ReadPressure();
Debug.WriteLine($"Pressure: {preValue.Hectopascals:0.##}hPa");

// Note that if you already have the pressure value and the temperature, you could also calculate altitude by
// calling WeatherHelper.CalculateAltitude(preValue, Pressure.MeanSeaLevel, tempValue) which would be more performant.
Length altValue = i2cBmp280.ReadAltitude(WeatherHelper.MeanSeaLevel);

Debug.WriteLine($"Altitude: {altValue:0.##}m");
Thread.Sleep(1000);

// set higher sampling
i2cBmp280.SetSampling(Sampling.UltraLowPower);

// read values
tempValue = i2cBmp280.ReadTemperature();
Debug.WriteLine($"Temperature: {tempValue.DegreesCelsius:0.#}\u00B0C");
preValue = i2cBmp280.ReadPressure();
Debug.WriteLine($"Pressure: {preValue.Hectopascals:0.##}hPa");

// Note that if you already have the pressure value and the temperature, you could also calculate altitude by
// calling WeatherHelper.CalculateAltitude(preValue, Pressure.MeanSeaLevel, tempValue) which would be more performant.
altValue = i2cBmp280.ReadAltitude(WeatherHelper.MeanSeaLevel);
Debug.WriteLine($"Altitude: {altValue:0.##}m");
