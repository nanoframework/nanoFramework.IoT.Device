// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;
using Iot.Device.KeyMatrix;

Debug.WriteLine("Please enter your output pins separated by a coma. For example: 27,22,25,6");
int[] outputs = new int[] { 27, 22, 25, 6 };
Debug.WriteLine("Please enter your input pins separated by a coma. For example: 17,23,24,5");
int[] inputs = new int[] { 17, 23, 24, 5 };
Debug.WriteLine("Please enter the scanning interval in milliseconds. For example: 15");
int interval = 15;
Debug.WriteLine("Please enter the number of keys you want to read individually events. For example: 20");
int count = 20;

// initialize keyboard
KeyMatrix mk = new KeyMatrix(outputs, inputs, TimeSpan.FromMilliseconds(interval));

// read key events
for (int n = 0; n < count; n++)
{
    Debug.WriteLine($"Waiting for matrix keyboard event... {n}/{count}");
    KeyMatrixEvent? key = mk.ReadKey();
    if (key is not object)
    {
        Debug.WriteLine("No key pressed");
        continue;
    }

    ShowKeyMatrixEvent(mk, key);
}

Debug.WriteLine("This will now start listening to events and display them for 60 seconds");
mk.KeyEvent += KeyMatrixEventReceived;
mk.StartListeningKeyEvent();
Thread.Sleep(60000);

mk.StopListeningKeyEvent();

// dispose
Debug.WriteLine("Dispose after 2 seconds...");
Thread.Sleep(2000);
mk.Dispose();

void KeyMatrixEventReceived(object sender, KeyMatrixEvent keyMatrixEvent)
{
    ShowKeyMatrixEvent((KeyMatrix)sender, keyMatrixEvent);
}

void ShowKeyMatrixEvent(KeyMatrix sender, KeyMatrixEvent pinValueChangedEventArgs)
{
    Debug.WriteLine($"{DateTime.UtcNow} {pinValueChangedEventArgs.Output}, {pinValueChangedEventArgs.Input}, {pinValueChangedEventArgs.EventType}");
    Debug.WriteLine("");

    // print keyboard status
    for (int r = 0; r < sender.OutputPins.Length; r++)
    {
        SpanPinValue rv = sender[r];
        for (int c = 0; c < sender.InputPins.Length; c++)
        {
            Debug.Write(rv[c] == PinValue.Low ? " ." : " #");
        }

        Debug.WriteLine("");
    }
}
