// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BlueNrg2.Aci
{
    public enum SecureConnectionSupport : byte
    {
        NotSupported = 0x00,
        Supported = 0x01,
        Mandatory = 0x02
    }
}
