// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using Iot.Device.MulticastDNS.Enum;
using Iot.Device.MulticastDNS.Package;

namespace Iot.Device.MulticastDNS.Entities
{
    /// <summary>
    /// Represents an AAAARecord Resource (DNS Resource Type 28).
    /// </summary>
    public class AAAARecord : AddressResource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AAAARecord" /> class.
        /// </summary>
        /// <param name="domain">The domain this Record points to.</param>
        /// <param name="address">The IPV6 address that points to the domain.</param>
        /// <param name="ttl">The TTL of this resource.</param>
        public AAAARecord(string domain, IPAddress address, int ttl = 2000) : base(domain, DnsResourceType.AAAA, ttl)
            => Address = address;

        internal AAAARecord(PacketParser packet, string domain, int ttl, int length) : base(domain, DnsResourceType.AAAA, ttl)
            => Address = new IPAddress(packet.ReadBytes(length));
    }
}
