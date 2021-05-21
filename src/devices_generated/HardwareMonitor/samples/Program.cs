// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Globalization;
using System.Threading;
using Iot.Device.HardwareMonitor;
using UnitsNet;

Debug.WriteLine("Press any key to quit");

OpenHardwareMonitor hw = new OpenHardwareMonitor();
if (hw.GetSensorList().Count == 0)
{
    Debug.WriteLine("OpenHardwareMonitor is not running");
    return;
}

hw.EnableDerivedSensors();

while (!Console.KeyAvailable)
{
    Console.Clear();
    Debug.WriteLine("Showing all available sensors (press any key to quit)");
    var components = hw.GetHardwareComponents();
    foreach (var component in components)
    {
        Debug.WriteLine("--------------------------------------------------------------------");
        Debug.WriteLine($"{component.Name} Type {component.Type}, Path {component.Identifier}");
        Debug.WriteLine("--------------------------------------------------------------------");
        foreach (var sensor in hw.GetSensorList(component))
        {
            Console.Write($"{sensor.Name}: Path {sensor.Identifier}, Parent {sensor.Parent} ");
            if (sensor.TryGetValue(out IQuantity? quantity))
            {
                Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "{0}: {1:g}", quantity.Type, quantity));
            }
            else
            {
                Debug.WriteLine($"No data");
            }
        }
    }

    if (hw.TryGetAverageGpuTemperature(out Temperature gpuTemp) &&
        hw.TryGetAverageCpuTemperature(out Temperature cpuTemp))
    {
        Debug.WriteLine($"Averages: CPU temp {cpuTemp:s2}, GPU temp {gpuTemp:s2}, CPU Load {hw.GetCpuLoad()}");
    }

    Thread.Sleep(1000);
}
