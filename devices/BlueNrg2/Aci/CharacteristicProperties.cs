// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// The properties a characteristic can have as described by (Volume 3, Part G, section 3.3.1.1 of Bluetooth Specification 4.1).
    /// </summary>
    [Flags]
    public enum CharacteristicProperties : byte
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Broadcast bit.
        /// </summary>
        Broadcast = 0x01,

        /// <summary>
        /// Read bit.
        /// </summary>
        Read = 0x02,

        /// <summary>
        /// Write without response bit.
        /// </summary>
        WriteWithoutResponse = 0x04,

        /// <summary>
        /// Write bit.
        /// </summary>
        Write = 0x08,

        /// <summary>
        /// Notify bit.
        /// </summary>
        Notify = 0x10,

        /// <summary>
        /// Indicate bit.
        /// </summary>
        Indicate = 0x20,

        /// <summary>
        /// Authenticated signed write bit.
        /// </summary>
        SignedWrite = 0x40,

        /// <summary>
        /// Extended properties bit.
        /// </summary>
        Extended = 0x80
    }
}