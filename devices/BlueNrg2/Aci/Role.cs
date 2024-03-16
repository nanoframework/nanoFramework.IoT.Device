// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum Role : byte
    {
        Peripheral = 0x01,
        BroadCaster = 0x02,
        Central = 0x04,
        Observer = 0x08
    }
}
