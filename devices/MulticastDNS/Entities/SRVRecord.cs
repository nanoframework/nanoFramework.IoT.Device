// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.MulticastDNS.Enum;
using Iot.Device.MulticastDNS.Package;

namespace Iot.Device.MulticastDNS.Entities
{
    /// <summary>
    /// Represents a SRVRecord Resource (DNS Resource Type 33).
    /// </summary>
    public class SrvRecord : TargetResource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SrvRecord" /> class.
        /// </summary>
        /// <param name="domain">The domain this Record is about.</param>
        /// <param name="priority">The priority of this SRVRecord.</param>
        /// <param name="weight">The weight of this SRVRecord.</param>
        /// <param name="port">The port of this SRVRecord.</param>
        /// <param name="targetDomain">The targetDomain of this SRVRecord.</param>
        /// <param name="ttl">The TTL of this SRVRecord.</param>
        public SrvRecord(string domain, ushort priority, ushort weight, ushort port, string targetDomain, int ttl = 2000) : base(domain, DnsResourceType.SRV, ttl)
        {
            Priority = priority;
            Weight = weight;
            Port = port;
            Target = targetDomain;
        }

        internal SrvRecord(PacketParser packet, string domain, int ttl) : base(domain, DnsResourceType.SRV, ttl)
        {
            Priority = packet.ReadUShort();
            Weight = packet.ReadUShort();
            Port = packet.ReadUShort();
            Target = packet.ReadDomain();
        }

        /// <summary>
        /// Gets or sets the port of this SRVRecord.
        /// </summary>
        public ushort Port { get; set; }

        /// <summary>
        /// Gets or sets the priority of this SRVRecord.
        /// </summary>
        public ushort Priority { get; set; }

        /// <summary>
        /// Gets or sets the weight of this SRVRecord.
        /// </summary>
        public ushort Weight { get; set; }

        /// <summary>
        /// Returns a byte[] representation of this Resource.
        /// </summary>
        /// <returns>A byte[] representation of this Resource.</returns>
        protected override byte[] GetBytesInternal()
        {
            var packetBuilder = new PacketBuilder();
            packetBuilder.Add(Priority);
            packetBuilder.Add(Weight);
            packetBuilder.Add(Port);
            packetBuilder.Add(Target);
            return packetBuilder.GetBytes();
        }
    }
}
