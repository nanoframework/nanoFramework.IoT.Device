// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading;
using Iot.Device.Mhz19b;
using nanoFramework.Hardware.Esp32; //This is required for ESP32 only and will need to be altered to suit your board. See samples.
using UnitsNet;

//This is required for ESP32 only and will need to be altered to suit your board. See samples.
Configuration.SetPinFunction(32, DeviceFunction.COM2_RX);
Configuration.SetPinFunction(33, DeviceFunction.COM2_TX);

// Let the binding handle serial port setup:
Mhz19b sensor = new Mhz19b("COM2");

// Alternativly, create serial port using the setting acc. to datasheet, pg. 7, sec. general settings
//using SerialPort serialPort = new("COM2", 9600, Parity.None, 8, StopBits.One)
//{
//    ReadTimeout = 1000,
//    WriteTimeout = 1000
//};

//serialPort.Open();
//using Mhz19b sensor = new(serialPort.BaseStream, true);

// Switch ABM on (default).
// sensor.SetAutomaticBaselineCorrection(AbmState.On);

// Set sensor detection range to 2000ppm (default).
// sensor.SetSensorDetectionRange(DetectionRange.Range2000);

// Perform calibration 
// Step #1: perform zero point calibration
// Step #2: perform span point calibration at 2000ppm
// CAUTION: enable the following lines only if you know exactly what you do.
//          Consider also that zero point and span point calibration are performed
//          at different concentrations. The sensor requires up to 20 min to be
//          saturated at the target level.
// sensor.PerformZeroPointCalibration();
// ---- Now change to target concentration for span point.
// sensor.PerformSpanPointCalibration(VolumeConcentration.FromPartsPerMillion(2000));

// Continously read current concentration
while (true)
{
    try
    {
        VolumeConcentration reading = sensor.GetCo2Reading();
        Debug.WriteLine($"{reading.PartsPerMillion:F0} ppm");
    }
    catch (IOException e)
    {
        Debug.WriteLine("Concentration couldn't be read");
        Debug.WriteLine(e.Message);
        Debug.WriteLine(e.InnerException?.Message);
    }

    Thread.Sleep(1000);
}
