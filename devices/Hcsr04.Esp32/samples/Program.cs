// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Threading;
using Iot.Device.Hcsr04.Esp32;
using UnitsNet;

Debug.WriteLine("Hello Hcsr04 Sample!");

using Hcsr04 sonar = new(26, 27);
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
