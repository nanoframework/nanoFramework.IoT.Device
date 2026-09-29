// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Vcnl4040;
using Iot.Device.Vcnl4040.Definitions;
using nanoFramework.Hardware.Esp32;

Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new I2cConnectionSettings(1, Vcnl4040Device.DefaultI2cAddress);
using I2cDevice i2cDevice = I2cDevice.Create(settings);
using Vcnl4040Device sensor = new Vcnl4040Device(i2cDevice);

AmbientLightSensor ambientLight = sensor.AmbientLightSensor;
ProximitySensor proximity = sensor.ProximitySensor;

ambientLight.IntegrationTime = AlsIntegrationTime.Time160ms;
ambientLight.PowerOn = true;

proximity.ConfigureEmitter(new EmitterConfiguration(
    PsLedCurrent.I200mA,
    PsDuty.Duty40,
    PsIntegrationTime.Time8_0,
    PsMultiPulse.Pulse2));
proximity.ConfigureReceiver(new ReceiverConfiguration(false, 0, true, false));
proximity.PowerOn = true;

while (true)
{
    Debug.WriteLine($"Illuminance: {ambientLight.Illuminance.Lux:F2} lux");
    Debug.WriteLine($"Proximity reading: {proximity.Distance} counts");
    Debug.WriteLine($"White channel: {proximity.WhiteChannelReading}");
    Thread.Sleep(1000);
}
