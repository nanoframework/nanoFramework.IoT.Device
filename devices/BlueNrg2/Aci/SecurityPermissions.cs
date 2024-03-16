// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// Bit mask for security permissions.
    /// </summary>
    [Flags]
    public enum SecurityPermissions : byte
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Need authentication to read.
        /// </summary>
        AuthenticatedRead = 0x01,

        /// <summary>
        /// Need authorization to read.
        /// </summary>
        AuthorizedRead = 0x02,

        /// <summary>
        /// Need encryption to read.
        /// </summary>
        EncryptedRead = 0x04,

        /// <summary>
        /// Need authentication to write.
        /// </summary>
        AuthenticatedWrite = 0x08,

        /// <summary>
        /// Need authorization to write.
        /// </summary>
        AuthorizedWrite = 0x10,

        /// <summary>
        /// Need encryption to write.
        /// </summary>
        EncryptedWrite = 0x20
    }
}