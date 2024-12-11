// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.MulticastDns.Enum;
using Iot.Device.MulticastDns.Package;

namespace Iot.Device.MulticastDns.Entities
{
    /// <summary>
    /// Represents a CNAME Record Resource (DNS Resource Type 5).
    /// </summary>
    public class CnameRecord : TargetResource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CnameRecord" /> class.
        /// </summary>
        /// <param name="domain">The domain this Record is about.</param>
        /// <param name="targetDomain">The targetDomain which is a CNAME for the domain.</param>
        /// <param name="ttl">The TTL of this resource.</param>
        public CnameRecord(string domain, string targetDomain, int ttl = 2000) : base(domain, DnsResourceType.CNAME, ttl)
            => Target = targetDomain;

        internal CnameRecord(PacketParser packet, string domain, int ttl) : base(domain, DnsResourceType.CNAME, ttl)
            => Target = packet.ReadDomain();
    }
}
