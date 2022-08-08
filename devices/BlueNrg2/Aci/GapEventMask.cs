// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// Flags for building a GAP event mask.
    /// </summary>
    [Flags]
    public enum GapEventMask : ushort
    {
        /// <summary>
        /// No events.
        /// </summary>
        NoEvents = 0x0000,

        /// <summary>
        /// Limited discoverable event bit.
        /// </summary>
        LimitedDiscoverableEvent = 0x0001,

        /// <summary>
        /// Pairing complete event bit.
        /// </summary>
        PairingCompleteEvent = 0x0002,

        /// <summary>
        /// Pass key request event bit.
        /// </summary>
        PassKeyReqEvent = 0x0004,

        /// <summary>
        /// Authorization request event bit.
        /// </summary>
        AuthorizationReqEvent = 0x0008,

        /// <summary>
        /// Slave security initiated event bit.
        /// </summary>
        SlaveSecurityInitiatedEvent = 0x0010,

        /// <summary>
        /// Bond lost event bit.
        /// </summary>
        BondLostEvent = 0x0020,

        /// <summary>
        /// Process complete event bit.
        /// </summary>
        ProcCompleteEvent = 0x0080,

        /// <summary>
        /// L2CAP connection update request event bit.
        /// </summary>
        L2CapConnectionUpdateRequestEvent = 0x0100,

        /// <summary>
        /// L2CAP connection update response event bit.
        /// </summary>
        L2CapConnectionUpdateResponseEvent = 0x0200,

        /// <summary>
        /// L2CAP process timeout event bit.
        /// </summary>
        L2CapProcessTimeoutEvent = 0x0400,

        /// <summary>
        /// Address not resolved event bit.
        /// </summary>
        AddressNotResolvedEvent = 0x0800
    }
}
