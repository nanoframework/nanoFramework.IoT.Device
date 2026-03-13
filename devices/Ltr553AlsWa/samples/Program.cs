// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Ltr553AlsWa;

Debug.WriteLine("Hello LTR-553ALS-WA!");

//////////////////////////////////////////////////////////////////////
// When connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus.
// On the M5Stack CoreS3, the LTR-553ALS-WA is on the internal I2C bus.
// Configuration.SetPinFunction(12, DeviceFunction.I2C1_DATA);
// Configuration.SetPinFunction(11, DeviceFunction.I2C1_CLOCK);

using Ltr553AlsWa sensor = new(I2cDevice.Create(new I2cConnectionSettings(1, Ltr553AlsWa.DefaultI2cAddress)));
Debug.WriteLine($"Part ID: 0x{sensor.PartId:X2}, Manufacturer ID: 0x{sensor.ManufacturerId:X2}");

// Configure ALS settings
sensor.AlsGain = AlsGain.Gain1X;
sensor.AlsIntegrationTime = AlsIntegrationTime.Integration100Ms;
sensor.AlsMeasurementRate = AlsMeasurementRate.Rate500Ms;

// Configure PS LED settings
sensor.LedPulseFrequency = LedPulseFrequency.Frequency60kHz;
sensor.LedDutyCycle = LedDutyCycle.DutyCycle100Percent;
sensor.LedPeakCurrent = LedPeakCurrent.Current100mA;
sensor.LedPulseCount = 1;

// Enable both proximity and ambient light sensors
sensor.PsEnabled = true;
sensor.AlsEnabled = true;

// Allow the sensor to take a first measurement
Thread.Sleep(100);

while (true)
{
    try
    {
        ushort proximity = sensor.GetProximity(out bool saturated);
        sensor.GetAlsData(out ushort ch0, out ushort ch1);
        Debug.WriteLine($"Proximity: {proximity}{(saturated ? " [SATURATED]" : string.Empty)}, ALS CH0 (visible+IR): {ch0}, ALS CH1 (IR): {ch1}");
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"Exception: {ex.Message}");
    }

    Thread.Sleep(500);
}
