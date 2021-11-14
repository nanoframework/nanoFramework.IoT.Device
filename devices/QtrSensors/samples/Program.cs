// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.QtrSensors;
using System;
using System.Diagnostics;
using System.Threading;
using System.Device.Gpio;

Debug.WriteLine("Hello from QtrSensors!");

QtrAnalog qtr = new(new int[] { 33, 14, 26, 4, 13 }, new int[] { 15 });
// We will use all emitters and on
qtr.EmitterSelection = EmitterSelection.All;

// There is no calibration, let's start:
var calibOn = qtr.Calibrate(10, true);
Debug.WriteLine("Calibration on:");
DisplayCalib(calibOn);
var calibOff = qtr.Calibrate(10, false);
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
        Debug.Write($"{i}={values:N2} ");
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