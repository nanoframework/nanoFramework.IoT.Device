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

#if DEBUG

    /// <summary>
    /// Extensions for the <see cref="DhcpMessageType"/> enumeration.
    /// </summary>
    public static class DhcpMessageTypeExtensions
    {
        /// <summary>
        /// Converts the DHCP message type to a string.
        /// </summary>
        /// <param name="messageType">The DHCP message type.</param>
        /// <returns>The string representation of the DHCP message type.</returns>
        public static string AsString(this DhcpMessageType messageType)
        {
            switch (messageType)
            {
                case DhcpMessageType.Unknown:
                    return "Unknown";
                case DhcpMessageType.Discover:
                    return "Discover";
                case DhcpMessageType.Offer:
                    return "Offer";
                case DhcpMessageType.Request:
                    return "Request";
                case DhcpMessageType.Decline:
                    return "Decline";
                case DhcpMessageType.Ack:
                    return "Ack";
                case DhcpMessageType.Nak:
                    return "Nak";
                case DhcpMessageType.Release:
                    return "Release";
                case DhcpMessageType.Inform:
                    return "Inform";
                case DhcpMessageType.ForceRenew:
                    return "ForceRenew";
                case DhcpMessageType.LeaseQuery:
                    return "LeaseQuery";
                case DhcpMessageType.LeaseUnassigned:
                    return "LeaseUnassigned";
                case DhcpMessageType.LeaseUnknown:
                    return "LeaseUnknown";
                case DhcpMessageType.LeaseActive:
                    return "LeaseActive";
                default:
                    return "Unknown (" + ((byte)messageType).ToString() + ")";
            }
        }
    }

#endif
}
