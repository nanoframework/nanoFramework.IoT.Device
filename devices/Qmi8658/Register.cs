// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Qmi8658
{
    internal enum Register : byte
    {
        WhoAmI = 0x00,
        Control1 = 0x02,
        Control2 = 0x03,
        Control3 = 0x04,
        Control5 = 0x06,
        Control7 = 0x08,
        Status0 = 0x2E,
        TemperatureLow = 0x33,
        AccelerometerXLow = 0x35,
        GyroscopeXLow = 0x3B,
        Reset = 0x60,
    }
}
