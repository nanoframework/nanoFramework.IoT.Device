// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Lsm6Dsl
{
    internal enum Register : byte
    {
        WhoAmI = 0x0F,
        Control1Accelerometer = 0x10,
        Control2Gyroscope = 0x11,
        Control3 = 0x12,
        Status = 0x1E,
        TemperatureLow = 0x20,
        GyroscopeXLow = 0x22,
        AccelerometerXLow = 0x28,
    }
}