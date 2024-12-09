// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Iot.Device.MulticastDNS.Enum;
using Iot.Device.MulticastDNS.Package;

namespace Iot.Device.MulticastDNS.Entities
{
    /// <summary>
    /// Represents a SRVRecord Resource (DNS Resource Type 16).
    /// </summary>
    public class TXTRecord : Resource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TXTRecord" /> class.
        /// </summary>
        /// <param name="domain">The domain this Record is about.</param>
        /// <param name="txt">The text this resource represents.</param>
        /// <param name="ttl">The TTL of this SRVRecord.</param>
        public TXTRecord(string domain, string txt, int ttl = 2000) : base(domain, DnsResourceType.TXT, ttl)
            => Txt = txt;

        internal TXTRecord(PacketParser packet, string domain, int ttl) : base(domain, DnsResourceType.TXT, ttl)
            => Txt = packet.ReadString();

        /// <summary>
        /// Gets the text this resource represents.
        /// </summary>
        public string Txt { get; }

        /// <summary>
        /// Returns a byte[] representation of this Resource.
        /// </summary>
        /// <returns>A byte[] representation of this Resource.</returns>
        protected override byte[] GetBytesInternal() => Encoding.UTF8.GetBytes(Txt);
    }
}
