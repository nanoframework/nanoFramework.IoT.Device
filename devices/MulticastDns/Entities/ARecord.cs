// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using Iot.Device.MulticastDns.Enum;
using Iot.Device.MulticastDns.Package;

namespace Iot.Device.MulticastDns.Entities
{
    /// <summary>
    /// Represents an A Record Resource (DNS Resource Type 1).
    /// </summary>
    public class ARecord : AddressResource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ARecord" /> class.
        /// </summary>
        /// <param name="domain">The domain this Record points to.</param>
        /// <param name="address">The IPV4 address that points to the domain.</param>
        /// <param name="ttl">The TTL of this resource.</param>
        public ARecord(string domain, IPAddress address, int ttl = 2000) : base(domain, DnsResourceType.A, ttl)
            => Address = address;

        internal ARecord(PacketParser packet, string domain, int ttl, int length) : base(domain, DnsResourceType.A, ttl)
            => Address = new IPAddress(packet.ReadBytes(length));
    }
}
