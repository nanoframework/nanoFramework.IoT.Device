// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// Mask for when a characteristic should send notifications.
    /// </summary>
    [Flags]
    public enum CharacteristicEventMask : byte
    {
        /// <summary>
        /// Don't notify events.
        /// </summary>
        DontNotifyEvents = 0x00,

        /// <summary>
        /// Notify attribute write bit.
        /// </summary>
        NotifyAttributeWrite = 0x01,

        /// <summary>
        /// Notify write requests and wait for approval response bit.
        /// </summary>
        NotifyWriteRequestAndWaitForApprovalResponse = 0x02,

        /// <summary>
        /// Notify read request and wait for approval response bit.
        /// </summary>
        NotifyReadRequestAndWaitForApprovalResponse = 0x04
    }
}