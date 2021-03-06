// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Multiplexing;
using System.Diagnostics;
using System.Threading;

using Mbi5027 sr = new(Mbi5027PinMapping.Complete);

Debug.WriteLine($"Driver for {nameof(Mbi5027)}");
Debug.WriteLine($"Register bit length: {sr.BitLength}");

CheckCircuit(sr);
BinaryCounter(sr);
CheckCircuit(sr);
sr.ShiftClear();

void BinaryCounter(Mbi5027 sr)
{
    int endValue = 65_536;
    Debug.WriteLine($"Write 0 through {endValue}");
    int delay = 20;

    for (int i = 0; i < endValue; i++)
    {
        for (int j = (sr.BitLength / 8) - 1; j > 0; j--)
        {
            int shift = j * 8;
            int downShiftedValue = i >> shift;
            sr.ShiftByte((byte)downShiftedValue, false);
        }

        sr.ShiftByte((byte)i);
        Thread.Sleep(delay);       
    }
}

void CheckCircuit(Mbi5027 sr)
{
    Debug.WriteLine("Checking circuit");
    sr.EnableDetectionMode();

    int index = sr.BitLength - 1;

    foreach (var value in sr.ReadOutputErrorStatus())
    {
        Debug.WriteLine($"Bit {index--}: {value}");
    }

    sr.EnableNormalMode();
}

bool IsCanceled(ShiftRegister sr, CancellationTokenSource cancellationSource)
{
    if (cancellationSource.IsCancellationRequested)
    {
        sr.ShiftClear();
        return true;
    }

    return false;
}
