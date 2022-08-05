// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci
{
    [Flags]
    public enum HalEventMask : uint
    {
        None = 0x00000000,
        ScanRequestReportEvent = 0x00000001
    }
}
