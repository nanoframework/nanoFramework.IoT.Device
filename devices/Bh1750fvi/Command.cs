// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Bh1750fvi
{
    internal enum Command : byte
    {
        PowerDown = 0b00000000,
        PowerOn = 0b00000001,
        Reset = 0b00000111,
        MeasurementTimeHigh = 0b01000000,
        MeasurementTimeLow = 0b01100000,
    }
}
