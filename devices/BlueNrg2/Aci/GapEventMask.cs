// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci
{
    [Flags]
    public enum GapEventMask : ushort
    {
        NoEvents = 0x0000,
        LimitedDiscoverableEvent = 0x0001,
        PairingCompleteEvent = 0x0002,
        PassKeyReqEvent = 0x0004,
        AuthorizationReqEvent = 0x0008,
        SlaveSecurityInitiatedEvent = 0x0010,
        BondLostEvent = 0x0020,
        ProcCompleteEvent = 0x0080,
        L2CapConnectionUpdateRequestEvent = 0x0100,
        L2CapConnectionUpdateResponseEvent = 0x0200,
        L2CapProcessTimeoutEvent = 0x0400,
        AddressNotResolvedEvent = 0x0800
    }
}
