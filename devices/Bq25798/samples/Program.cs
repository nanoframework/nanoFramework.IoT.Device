// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Bq25798;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
//Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
//////////////////////////////////////////////////////////////////////

I2cConnectionSettings settings = new(1, Bq25798.DefaultI2cAddress);
using I2cDevice device = I2cDevice.Create(settings);
using Bq25798 charger = new(device);

Debug.WriteLine("");
Debug.WriteLine($"BQ25798 connected to I2C{device.ConnectionSettings.BusId}");
Debug.WriteLine("");

Debug.WriteLine($"Minimum System Voltage is config @ {charger.MinimalSystemVoltage.VoltsDc:N3}V");

while (true)
{
    Debug.WriteLine($"Current Vbus: {charger.Vbus.VoltsDc:N3}V");

    Debug.WriteLine("");

    Thread.Sleep(1000);
}
