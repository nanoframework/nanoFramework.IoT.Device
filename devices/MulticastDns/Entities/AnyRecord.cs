// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.MulticastDns.Enum;
using Iot.Device.MulticastDns.Package;

namespace Iot.Device.MulticastDns.Entities
{
    /// <summary>
    /// Represents a DNS resource record with type ANY (255).
    /// Since RR type 255 does not define a structured RDATA format, this implementation
    /// preserves the record data as raw bytes and returns it unchanged when serialized.
    /// </summary>
    public class AnyRecord : Resource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnyRecord" /> class.
        /// </summary>
        /// <param name="domain">The domain this Record is about.</param>
        /// <param name="ttl">The TTL of this resource.</param>
        public AnyRecord(string domain, int ttl = 2000) : base(domain, DnsResourceType.ANY, ttl)
            => Data = new byte[0];

        internal AnyRecord(PacketParser packet, string domain, int ttl, int length) : base(domain, DnsResourceType.ANY, ttl)
            => Data = packet.ReadBytes(length);

        /// <summary>
        /// Gets the raw data bytes of this record.
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// Returns a byte[] representation of this Resource.
        /// </summary>
        /// <returns>A byte[] representation of this Resource.</returns>
        protected override byte[] GetBytesInternal() => Data;
    }
}
