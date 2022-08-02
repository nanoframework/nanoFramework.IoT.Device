// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.DhcpServer.Enums
{
    /// <summary>
    /// DHCP Operation.
    /// </summary>
    public enum DhcpOperation : byte
    {
        /// <summary>Boot request.</summary>
        BootRequest = 0x01,

        /// <summary>Boot reply.</summary>
        BootReply
    }
}
