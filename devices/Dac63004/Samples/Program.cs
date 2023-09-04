// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Dac63004;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
//Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
//////////////////////////////////////////////////////////////////////

I2cConnectionSettings settings = new(3, Dac63004.DefaultI2cAddress);
using I2cDevice device = I2cDevice.Create(settings);
using Dac63004 dac = new(device);

Debug.WriteLine("");
Debug.WriteLine($"DAC63004 connected to I2C{device.ConnectionSettings.BusId}");
Debug.WriteLine("");

//Debug.WriteLine($"Minimum System Voltage is config @ {dac.MinimalSystemVoltage.VoltsDc:N3}V");

//while (true)
//{
//    Debug.WriteLine($"Vbus status is {dac.VbusStatus}");
    
//    Debug.WriteLine($"Current Vbus: {dac.Vbus.VoltsDc:N3}V");
//    Debug.WriteLine($"Current Vac1: {dac.Vac1.VoltsDc:N3}V");
//    Debug.WriteLine($"Current Vac2: {dac.Vac2.VoltsDc:N3}V");
//    Debug.WriteLine($"Current Vbat: {dac.Vbat.VoltsDc:N3}V");
//    Debug.WriteLine($"Current Vsys: {dac.Vsys.VoltsDc:N3}V");

//    Debug.WriteLine($"Die Temp: {dac.DieTemperature.DegreesCelsius:N1}°C");

//    Debug.WriteLine("");

//    Thread.Sleep(3000);
//}
