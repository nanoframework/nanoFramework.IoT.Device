// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Bq2579x;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
//Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
//////////////////////////////////////////////////////////////////////

I2cConnectionSettings settings = new(3, Bq2579x.DefaultI2cAddress);
using I2cDevice device = I2cDevice.Create(settings);
using Bq2579x charger = new(device);

Debug.WriteLine("");
Debug.WriteLine($"{(charger.Model == Model.Bq25792 ? "Bq25792" : "Bq25798")} connected to I2C{device.ConnectionSettings.BusId}");
Debug.WriteLine("");

// disabling I2C watchog for simplicity
charger.WatchdogTimerSetting = WatchdogSetting.Disable;

Debug.WriteLine($"Minimum System Voltage is config @ {charger.MinimalSystemVoltage.VoltsDc:N3}V");

while (true)
{
    Debug.WriteLine($"Vbus status is {charger.VbusStatus}");
    
    Debug.WriteLine($"Current Vbus: {charger.Vbus.VoltsDc:N3}V");
    Debug.WriteLine($"Current Vac1: {charger.Vac1.VoltsDc:N3}V");
    Debug.WriteLine($"Current Vac2: {charger.Vac2.VoltsDc:N3}V");
    Debug.WriteLine($"Current Vbat: {charger.Vbat.VoltsDc:N3}V");
    Debug.WriteLine($"Current Vsys: {charger.Vsys.VoltsDc:N3}V");

    Debug.WriteLine($"Die Temp: {charger.DieTemperature.DegreesCelsius:N1}°C");

    Debug.WriteLine("");

    Thread.Sleep(3000);
}
