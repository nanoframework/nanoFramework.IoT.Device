// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.DhcpServer.Enums
{
    /// <summary>
    /// The type of DHCP Messages.
    /// </summary>
    public enum DhcpMessageType : byte
    {
        /// <summary>Unknown.</summary>
        Unknown = 0x00,

        /// <summary>Discover.</summary>
        Discover,

        /// <summary>Offer.</summary>
        Offer,

        /// <summary>Request.</summary>
        Request,

        /// <summary>Decline.</summary>
        Decline,

        /// <summary>Acknoledged.</summary>
        Ack,

        /// <summary>Not akcknoledged.</summary>
        Nak,

        /// <summary>Release.</summary>
        Release,

        /// <summary>Inform.</summary>
        Inform,

        /// <summary>Force renew.</summary>
        ForceRenew,

        /// <summary>Lease query.</summary>
        LeaseQuery,

        /// <summary>Lease unassigned.</summary>
        LeaseUnassigned,

        /// <summary>Lease unknown.</summary>
        LeaseUnknown,

        /// <summary>Lease active.</summary>
        LeaseActive
    }
}
