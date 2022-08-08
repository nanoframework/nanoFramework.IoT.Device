// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BlueNrg2.Aci
{
    public enum Offset : byte
    {
        BluetoothPublicAddress = 0x00,
        CsrkDerivingDivider = 0x06,
        EncryptionRootKey = 0x08,
        IdentityRootKey = 0x18,
        LinkLayerWithoutHost = 0x2c,
        StaticRandomAddress = 0x2e,
        DisableWatchdog = 0x2f,
        UseDebugKey = 0xd0,
        MaximumAllowedValuesForDataLengthExtension = 0xd1
    }
}
