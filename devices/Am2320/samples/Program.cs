// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Am2320;
using nanoFramework.Hardware.Esp32;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

Debug.WriteLine("Hello from AM2320!");

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

using Am2320 am2330 = new(new I2cDevice(new I2cConnectionSettings(1, Am2320.DefaultI2cAddress, I2cBusSpeed.StandardMode)));

// On some copies, the device information contains only 0
var deviceInfo = am2330.DeviceInformation;
if (deviceInfo != null)
{
    Debug.WriteLine($"Model: {deviceInfo.Model}");
    Debug.WriteLine($"Version: {deviceInfo.Version}");
    Debug.WriteLine($"Device ID: {deviceInfo.DeviceId}");
}

while(true)
{
    var temp = am2330.Temperature;
    var hum = am2330.Humidity;
    if(am2330.IsLastReadSuccessful)
    {
        Debug.WriteLine($"Temp = {temp.DegreesCelsius} C, Hum = {hum.Percent} %");
    }
    else
    {
        Debug.WriteLine("Not sucessful reading.");
    }

    Thread.Sleep(Am2320.MinimumReadPeriod);
}

Thread.Sleep(Timeout.Infinite);
