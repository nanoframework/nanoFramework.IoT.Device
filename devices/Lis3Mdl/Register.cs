// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Lis3Mdl
{
    internal enum Register : byte
    {
        WhoAmI = 0x0F,
        Control1 = 0x20,
        Control2 = 0x21,
        Control3 = 0x22,
        Control4 = 0x23,
        Control5 = 0x24,
        Status = 0x27,
        OutputXLow = 0x28,
        TemperatureLow = 0x2E,
    }
}