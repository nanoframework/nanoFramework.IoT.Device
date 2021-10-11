// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Bh1745;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
//Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

// create device
I2cConnectionSettings i2cSettings = new I2cConnectionSettings(1, Bh1745.DefaultI2cAddress);
var i2cDevice = I2cDevice.Create(i2cSettings);

using Bh1745 i2cBh1745 = new Bh1745(i2cDevice)
{
    // multipliers affect the compensated values
    // ChannelCompensationMultipliers:  Red, Green, Blue, Clear
    ChannelCompensationMultipliers = new(2.5, 0.9, 1.9, 9.5),

    // set custom  measurement time
    MeasurementTime = MeasurementTime.Ms1280,

    // interrupt functionality is detailed in the datasheet
    // Reference: https://www.mouser.co.uk/datasheet/2/348/bh1745nuc-e-519994.pdf (page 13)
    LowerInterruptThreshold = 0xABFF,
    HigherInterruptThreshold = 0x0A10,

    LatchBehavior = LatchBehavior.LatchEachMeasurement,
    InterruptPersistence = InterruptPersistence.UpdateMeasurementEnd,
    InterruptIsEnabled = true,
};

// wait for first measurement
Thread.Sleep(i2cBh1745.MeasurementTimeAsTimeSpan());

while (true)
{
    var color = i2cBh1745.GetCompensatedColor();

    if (!i2cBh1745.ReadMeasurementIsValid())
    {
        Debug.WriteLine("Measurement was not valid!");
        continue;
    }

    Debug.WriteLine($"RGB color read: #{color.R:X2}{color.G:X2}{color.B:X2}");
    Debug.WriteLine($"Raw illumination value: {i2cBh1745.ReadClearDataRegister()}");

    Thread.Sleep(i2cBh1745.MeasurementTimeAsTimeSpan());
}
