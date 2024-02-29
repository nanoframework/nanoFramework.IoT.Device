// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Lps22Hb;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
//Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

using Lps22Hb lps22HdDevice = new(CreateI2cDevice(), FifoMode.Bypass);

while (true)
{
    var tempValue = lps22HdDevice.Temperature;
    var pressure = lps22HdDevice.Pressure;

    Debug.WriteLine($"Temperature: {tempValue.DegreesCelsius:F1}\u00B0C");
    Debug.WriteLine($"Pressure: {pressure.Hectopascals:F1}hPa");

    Thread.Sleep(1000);
}

I2cDevice CreateI2cDevice()
{
    I2cConnectionSettings settings = new(1, Lps22Hb.I2cAddress);
    return I2cDevice.Create(settings);
}
