// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Multiplexing;

int[] pins = new int[] { 4, 17, 27, 22 };
int charlieSegmentLength = 8;
bool controlCRequested = false;
int twoSeconds = 2000;

// calling this method helps with determing the correct pin circuit to use
CharlieplexSegmentNode[] nodes = CharlieplexSegment.GetNodes(pins, charlieSegmentLength);
for (int i = 0; i < charlieSegmentLength; i++)
{
    CharlieplexSegmentNode node = nodes[i];
    Debug.WriteLine($"Node {i} -- Anode: {node.Anode}; Cathode: {node.Cathode}");
}

using CharlieplexSegment segment = new(pins, charlieSegmentLength);

CancellationTokenSource cts = new();

Debug.WriteLine("Light all LEDs");
for (int i = 0; i < charlieSegmentLength; i++)
{
    segment.Write(i, 1);
}

DisplayShouldCancel();

Debug.WriteLine("Dim all LEDs");
for (int i = 0; i < charlieSegmentLength; i++)
{
    segment.Write(i, 0);
}

Debug.WriteLine("Light odd values");
for (int i = 0; i < charlieSegmentLength; i++)
{
    if (i % 2 == 1)
    {
        segment.Write(i, 1);
    }
}

DisplayShouldCancel();

for (int i = 0; i < charlieSegmentLength; i++)
{
    segment.Write(i, 0);
}

var delayLengths = new int[] { 1, 5, 10, 25, 50, 100, 250, 500, 1000 };
foreach (var delay in delayLengths)
{
    Debug.WriteLine($"Light one LED at a time -- Delay {delay}");
    for (int i = 0; i < charlieSegmentLength; i++)
    {
        segment.Write(i, 1);
        DisplayShouldCancel(delay);
        segment.Write(i, 0);
        DisplayShouldCancel(delay / 2);
    }
}

Reverse(delayLengths);
foreach (var delay in delayLengths)
{
    Debug.WriteLine($"Light and then dim all LEDs, in sequence. Delay: {delay}");
    for (int i = 0; i < charlieSegmentLength; i++)
    {
        segment.Write(i, 1);
        DisplayShouldCancel(delay);
    }

    for (int i = 0; i < charlieSegmentLength; i++)
    {
        segment.Write(i, 0);
        DisplayShouldCancel(delay / 2);
    }
}

void DisplayShouldCancel(int delay = -1)
{

    if (delay == -1)
    {
        delay = twoSeconds;
    }

    cts.CancelAfter(delay);
    segment.Display(cts.Token);
}

segment.Dispose();

void Reverse(int[] array)
{
    int[] reversed = new int[array.Length];
    for (int i = 0; i < array.Length; i++)
    {
        reversed[reversed.Length - 1 - i] = array[i];
    }

    array = reversed;
}