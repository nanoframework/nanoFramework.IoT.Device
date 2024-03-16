// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// Values of channels where overflow can occur.
    /// </summary>
    public enum LinkType : byte
    {
        /// <summary>
        /// ACL buffer overflow.
        /// </summary>
        Acl = 0x01,
    }
}
