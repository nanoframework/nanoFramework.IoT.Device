// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Threading.Tasks;
using Iot.Device.OneWire;

// Make sure you can access the bus device before requesting a device scan (or run using sudo)
// $ sudo chmod a+rw /sys/bus/w1/devices/w1_bus_master1/w1_master_*
if (args.Any(_ => _ == "temp"))
{
    // Quick and simple way to find a thermometer and print the temperature
    foreach (var dev in OneWireThermometerDevice.EnumerateDevices())
    {
        Debug.WriteLine($"Temperature reported by '{dev.DeviceId}': " +
                            (await dev.ReadTemperatureAsync()).DegreesCelsius.ToString("F2") + "\u00B0C");
    }
}
else
{
    // More advanced way, with rescanning the bus and iterating devices per 1-wire bus
    foreach (string busId in OneWireBus.EnumerateBusIds())
    {
        OneWireBus bus = new(busId);
        Debug.WriteLine($"Found bus '{bus.BusId}', scanning for devices ...");
        await bus.ScanForDeviceChangesAsync();
        foreach (string devId in bus.EnumerateDeviceIds())
        {
            OneWireDevice dev = new(busId, devId);
            Debug.WriteLine($"Found family '{dev.Family}' device '{dev.DeviceId}' on '{bus.BusId}'");
            if (OneWireThermometerDevice.IsCompatible(busId, devId))
            {
                OneWireThermometerDevice devTemp = new(busId, devId);
                Debug.WriteLine("Temperature reported by device: " +
                                    (await devTemp.ReadTemperatureAsync()).DegreesCelsius.ToString("F2") +
                                    "\u00B0C");
            }
        }
    }
}
