# Iot.Device.Multiplexing

Generic helper implementation for pin multiplexing. See [ShiftRegister](../ShiftRegister) or [Sn74hc595](../Sn74hc595).

## Usage

```csharp
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Threading;
using Iot.Device.Multiplexing;

int[] pins = new int[] { 4, 17, 27, 22, 5, 6, 13, 19 };
using IOutputSegment segment = new GpioOutputSegment(pins);

CancellationTokenSource cts = new();
CancellationToken token = cts.Token;
bool controlCRequested = false;
TimeSpan delay = TimeSpan.FromSeconds(5);

WriteLine("Light all LEDs");
segment.TurnOffAll();
for (int i = 0; i < pins.Length; i++)
{
    segment.Write(i, 1);
}

WriteLine("Light every other LED");
segment.TurnOffAll();
for (int i = 0; i < pins.Length; i++)
{
    segment.Write(i, i % 2);
}

WriteLine("Light every other (other) LED");
segment.TurnOffAll();
for (int i = 0; i < pins.Length; i++)
{
    segment.Write(i, (i + 1) % 2);
}

WriteLine("Display binary 128");
segment.TurnOffAll();
for (int i = 0; i < pins.Length; i++)
{
    segment.Write(128);
}

segment.TurnOffAll();
WriteLine("Done.");
```
