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
        internal AnyRecord(PacketParser packet, string domain, int ttl, int length) : base(domain, DnsResourceType.ANY, ttl)
            => _data = packet.ReadBytes(length);

        private readonly byte[] _data;

        /// <summary>
        /// Gets the raw data bytes of this record.
        /// </summary>
        public byte[] Data => (byte[])_data.Clone();

        /// <summary>
        /// Returns a byte[] representation of this Resource.
        /// </summary>
        /// <returns>A byte[] representation of this Resource.</returns>
        protected override byte[] GetBytesInternal() => _data;
    }
}
