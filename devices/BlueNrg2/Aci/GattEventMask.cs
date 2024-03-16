// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// Event mask for GATT events.
    /// </summary>
    [Flags]
    public enum GattEventMask : uint
    {
        /// <summary>
        /// Attribute modified event bit.
        /// </summary>
        AttributeModifiedEvent = 0x00000001,

        /// <summary>
        /// Process timeout event bit.
        /// </summary>
        ProcTimeoutEvent = 0x00000002,

        /// <summary>
        /// Exchange MTU response event bit.
        /// </summary>
        ExchangeMtuRespEvent = 0x00000004,

        /// <summary>
        /// Find info response event bit.
        /// </summary>
        FindInfoRespEvent = 0x00000008,

        /// <summary>
        /// Find by type value response event bit.
        /// </summary>
        FindByTypeValueRespEvent = 0x00000010,

        /// <summary>
        /// Read by type response event bit.
        /// </summary>
        ReadByTypeRespEvent = 0x00000020,

        /// <summary>
        /// Read response event bit.
        /// </summary>
        ReadRespEvent = 0x00000040,

        /// <summary>
        /// Read blob response event bit.
        /// </summary>
        ReadBlobRespEvent = 0x00000080,

        /// <summary>
        /// Read multiple response event bit.
        /// </summary>
        ReadMultipleRespEvent = 0x00000100,

        /// <summary>
        /// Read by group type response event bit.
        /// </summary>
        ReadByGroupTypeRespEvent = 0x00000200,

        /// <summary>
        /// Prepare write response event bit.
        /// </summary>
        PrepareWriteRespEvent = 0x00000800,

        /// <summary>
        /// Execute write response event bit.
        /// </summary>
        ExecWriteRespEvent = 0x00001000,

        /// <summary>
        /// Indication event bit.
        /// </summary>
        IndicationEvent = 0x00002000,

        /// <summary>
        /// Notification event bit.
        /// </summary>
        NotificationEvent = 0x00004000,

        /// <summary>
        /// Error response event bit.
        /// </summary>
        ErrorRespEvent = 0x00008000,

        /// <summary>
        /// Process complete event bit.
        /// </summary>
        ProcCompleteEvent = 0x00010000,

        /// <summary>
        /// Discover read characteristic by UUID response event bit.
        /// </summary>
        DiscReadCharByUuidRespEvent = 0x00020000,

        /// <summary>
        /// Transmitter pool available event bit.
        /// </summary>
        TxPoolAvailableEvent = 0x00040000
    }
}