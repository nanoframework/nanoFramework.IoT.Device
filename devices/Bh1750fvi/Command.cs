// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Bh1750fvi
{
    internal enum Command : byte
    {
        PowerDown = 0b0000_0000,
        PowerOn = 0b0000_0001,
        Reset = 0b0000_0111,
        MeasurementTimeHigh = 0b0100_0000,
        MeasurementTimeLow = 0b0110_0000,
    }
}
