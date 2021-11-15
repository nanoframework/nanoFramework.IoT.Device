// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Hcsr04;
using UnitsNet;

Debug.WriteLine("Hello Hcsr04 Sample!");

// You can use as well the same pin: using Hcsr04 sonar = new(5, 5);
using Hcsr04 sonar = new(12, 14);
while (true)
{
    if (sonar.TryGetDistance(out Length distance))
    {
        Debug.WriteLine($"Distance: {distance.Centimeters} cm");
    }
    else
    {
        Debug.WriteLine("Error reading sensor");
    }

    Thread.Sleep(1000);
}
