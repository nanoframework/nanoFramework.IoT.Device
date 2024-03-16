// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// Bitmask specifying the type of update generated.
    /// </summary>
    [Flags]
    public enum UpdateType : byte
    {
        /// <summary>
        /// Local update.
        /// </summary>
        LocalUpdate = 0x00,

        /// <summary>
        /// Notification bit.
        /// </summary>
        Notification = 0x01,

        /// <summary>
        /// Indication bit.
        /// </summary>
        Indication = 0x02,

        /// <summary>
        /// Disable retransmission bit.
        /// </summary>
        DisableRetransmission = 0x04
    }
}