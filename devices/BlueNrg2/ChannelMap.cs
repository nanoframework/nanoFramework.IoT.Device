// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2
{
    [Flags]
    public enum ChannelMap : byte
    {
        C37 = 0x01,
        C38 = 0x02,
        C39 = 0x04
    }
}
