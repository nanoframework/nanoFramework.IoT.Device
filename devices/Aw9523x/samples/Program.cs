// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Aw9523x;
using nanoFramework.Hardware.Esp32;

Debug.WriteLine("AW9523X sample for M5Stack CoreS3");

Configuration.SetPinFunction(12, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(11, DeviceFunction.I2C1_CLOCK);

using Aw9523x aw9523 = new(I2cDevice.Create(new I2cConnectionSettings(1, Aw9523x.DefaultI2cAddress)));

const OutputMask coreS3BusEn = OutputMask.PortBit1;
const OutputMask coreS3UsbOtgEn = OutputMask.PortBit5;
const OutputMask coreS3BoostEn = OutputMask.PortBit7;

Debug.WriteLine($"Chip ID: 0x{aw9523.ChipId:X2} (expected 0x{Aw9523x.ExpectedChipId:X2})");
if (!aw9523.IsAw9523x)
{
    Debug.WriteLine("Warning: unexpected chip ID. Check board wiring and I2C bus.");
}

SetCoreS3Output(aw9523, coreS3BusEn, true, coreS3BusEn, coreS3BoostEn);
Debug.WriteLine("CoreS3 internal BUS_EN asserted.");

SetCoreS3Output(aw9523, coreS3UsbOtgEn, true, coreS3BusEn, coreS3BoostEn);
Debug.WriteLine("CoreS3 USB OTG_EN asserted.");

while (true)
{
    byte p0 = aw9523.ReadOutputPort(Port.Port0);
    byte p1 = aw9523.ReadOutputPort(Port.Port1);
    Debug.WriteLine($"Port0 OUT: 0x{p0:X2}, Port1 OUT: 0x{p1:X2}");
    Thread.Sleep(1000);
}

static void SetCoreS3Output(Aw9523x aw9523, OutputMask mask, bool enable, OutputMask busEnableMask, OutputMask boostEnableMask)
{
    if (mask == OutputMask.None)
    {
        throw new System.ArgumentOutOfRangeException(nameof(mask));
    }

    if (busEnableMask == OutputMask.None)
    {
        throw new System.ArgumentOutOfRangeException(nameof(busEnableMask));
    }

    if (boostEnableMask == OutputMask.None)
    {
        throw new System.ArgumentOutOfRangeException(nameof(boostEnableMask));
    }

    byte port0 = aw9523.ReadOutputPort(Port.Port0);
    byte port1 = aw9523.ReadOutputPort(Port.Port1);
    byte maskByte = (byte)mask;
    byte busEnableByte = (byte)busEnableMask;
    byte boostEnableByte = (byte)boostEnableMask;

    if (enable)
    {
        port0 |= maskByte;
        port1 |= boostEnableByte;
    }
    else
    {
        port0 &= (byte)~maskByte;
        if ((port0 & busEnableByte) == 0)
        {
            port1 &= unchecked((byte)~boostEnableByte);
        }
    }

    aw9523.WriteOutputPort(Port.Port0, port0);
    aw9523.WriteOutputPort(Port.Port1, port1);
}
