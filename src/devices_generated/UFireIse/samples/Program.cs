// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.UFire;
using UnitsNet;

const int BusId = 1;

PrintHelp();

I2cConnectionSettings settings = new(BusId, UFireIse.I2cAddress);
using I2cDevice device = I2cDevice.Create(settings);
Debug.WriteLine(
        $"UFire_ISE is ready on I2C bus {device.ConnectionSettings.BusId} with address {device.ConnectionSettings.DeviceAddress}");

Debug.WriteLine("");

while (true)
{
    string[]? command = Console.ReadLine()?.ToLower()?.Split(' ');
    if (command?[0] is not { Length: >0 })
    {
        return;
    }

    switch (command[0][0])
    {
        case 'b':
            Basic(device);
            return;
        case '0':
            Orp(device);
            return;
        case 'p':
            Ph(device);
            return;
    }
}

void PrintHelp()
{
    Debug.WriteLine("Command:");
    Debug.WriteLine("    B           Basic");
    Debug.WriteLine("    O           Read Orp (Oxidation-reduction potential) value");
    Debug.WriteLine("    P           Read pH (Power of Hydrogen) value");
    Debug.WriteLine("");
}

void Basic(I2cDevice device)
{
    using UFireIse uFireIse = new UFireIse(device);
    Debug.WriteLine("mV:" + uFireIse.ReadElectricPotential().Millivolts);
}

void Orp(I2cDevice device)
{
    using UFireOrp uFireOrp = new(device);
    if (uFireOrp.TryMeasureOxidationReductionPotential(out ElectricPotential orp))
    {
        Debug.WriteLine("Eh:" + orp.Millivolts);
    }
    else
    {
        Debug.WriteLine("Not possible to measure pH");
    }
}

void Ph(I2cDevice device)
{
    using UFirePh uFire_pH = new UFirePh(device);
    Debug.WriteLine("mV:" + uFire_pH.ReadElectricPotential().Millivolts);

    if (uFire_pH.TryMeasurepH(out float pH))
    {
        Debug.WriteLine("pH:" + pH);
    }
    else
    {
        Debug.WriteLine("Not possible to measure pH");
    }
}
