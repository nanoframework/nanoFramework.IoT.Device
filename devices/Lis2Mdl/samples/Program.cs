// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Lis2Mdl;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
//Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

using Lis2Mdl lis2mdlDevice = new(CreateI2cDevice());

while (true)
{
    var tempValue = lis2mdlDevice.Temperature;
    var magFieldValue = lis2mdlDevice.MagneticField;

    Debug.WriteLine($"Temperature: {tempValue.DegreesCelsius:F1}\u00B0C");
    Debug.WriteLine($"Mag. field X: {UnitsNet.MagneticField.FromMilligausses(magFieldValue.X).Milligausses:F3}mG");
    Debug.WriteLine($"Mag. field Y: {UnitsNet.MagneticField.FromMilligausses(magFieldValue.Y).Milligausses:F3}mG");
    Debug.WriteLine($"Mag. field Z: {UnitsNet.MagneticField.FromMilligausses(magFieldValue.Z).Milligausses:F3}mG");

    Thread.Sleep(1000);
}

I2cDevice CreateI2cDevice()
{
    I2cConnectionSettings settings = new(1, Lis2Mdl.I2cAddress);
    return I2cDevice.Create(settings);
}
