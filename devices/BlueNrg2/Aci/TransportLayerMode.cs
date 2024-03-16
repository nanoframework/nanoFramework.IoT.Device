// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci
{
    [Flags]
    public enum TransportLayerMode : byte
    {
        Uart = 0x01,
        Spi = 0x02
    }
}
