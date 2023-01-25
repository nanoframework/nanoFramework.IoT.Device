// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.QtrSensors;
using System;
using System.Diagnostics;
using System.Threading;
using System.Device.Gpio;
using nanoFramework.Hardware.Esp32;

Debug.WriteLine("Hello from QtrSensors!");

Configuration.SetPinFunction(32, DeviceFunction.ADC1_CH1);
Configuration.SetPinFunction(14, DeviceFunction.ADC1_CH2);
Configuration.SetPinFunction(26, DeviceFunction.ADC1_CH4);
Configuration.SetPinFunction(4, DeviceFunction.ADC1_CH5);
Configuration.SetPinFunction(13, DeviceFunction.ADC1_CH6);

QtrAnalog qtr = new(new int[] { 1, 2, 4, 5, 6 }, new int[] { 15 });
// We will use all emitters and on
qtr.EmitterSelection = EmitterSelection.All;

// There is no calibration, let's start:
var calibOn = qtr.Calibrate(100, true);
Debug.WriteLine("Calibration on:");
DisplayCalib(calibOn);
var calibOff = qtr.Calibrate(100, false);
Debug.WriteLine("Calibration off:");
DisplayCalib(calibOn);

qtr.EmitterValue = PinValue.High;
for (int idx = 0; idx < 2; idx++)
{

    // Gets raw values
    Debug.WriteLine($"Read raws, emitter {qtr.EmitterValue}:");
    double[] raws;
    double pos;
    for (int i = 1; i < 100; i++)
    {
        raws = qtr.ReadRaw();
        DisplayValues(raws);
    }

    Debug.WriteLine($"Read ratio, emitter {qtr.EmitterValue}:");
    for (int i = 1; i < 100; i++)
    {
        raws = qtr.ReadRatio();
        DisplayValues(raws);
    }

    Debug.WriteLine($"Read position black lines, emitter {qtr.EmitterValue}:");
    for (int i = 1; i < 100; i++)
    {
        pos = qtr.ReadPosition();
        Debug.WriteLine($"Pos={pos:N2}");
    }

    qtr.EmitterValue = qtr.EmitterValue == PinValue.High ? PinValue.Low : PinValue.High;
}


Thread.Sleep(Timeout.Infinite);

void DisplayValues(double[] values)
{
    for (int i = 0; i < values.Length; i++)
    {
        Debug.Write($"{i}={values[i]:N2} ");
    }

    Debug.WriteLine("");
}

void DisplayCalib(CalibrationData[] calib)
{
    for (int i = 0; i < calib.Length; i++)
    {
        Debug.WriteLine($"  {i}: Min={calib[i].MinimumValue:N2} Max={calib[i].MaximumValue:N2}");
    }
}

int GpioToAdcChannel(int gpio) => gpio switch
{
    36 => 0,
    37 => 1,
    38 => 2,
    39 => 3,
    32 => 4,
    33 => 5,
    34 => 6,
    35 => 7,
    4 => 10,
    0 => 11,
    2 => 12,
    15 => 13,
    13 => 14,
    12 => 15,
    //14 => 16,
    14 => 0,
    27 => 17,
    25 => 18,
    //26 => 19,
    26 => 1,
    _ => -1
};
