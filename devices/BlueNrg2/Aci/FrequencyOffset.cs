// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BlueNrg2.Aci
{
    public enum FrequencyOffset : byte
    {
        NoOffset = 0x00,
        Plus250Khz = 0x01,
        Minus250Khz = 0x02
    }
}
