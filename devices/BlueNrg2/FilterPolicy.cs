// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BlueNrg2
{
    public enum FilterPolicy
    {
        ScanAnyRequestAny = 0x00,
        ScanWhitelistRequestAny = 0x01,
        ScanAnyRequestWhitelist = 0x02,
        ScanWhitelistRequestWhitelist = 0x03
    }
}
