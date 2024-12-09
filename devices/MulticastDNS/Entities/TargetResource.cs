// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.MulticastDNS.Enum;
using Iot.Device.MulticastDNS.Package;

namespace Iot.Device.MulticastDNS.Entities
{
    /// <summary>
    /// The base class for a Target Resource.
    /// </summary>
    public abstract class TargetResource : Resource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TargetResource" /> class.
        /// </summary>
        /// <param name="domain">The domain this Record points to.</param>
        /// <param name="type">The type of this resource.</param>
        /// <param name="ttl">The TTL of this resource.</param>
        public TargetResource(string domain, DnsResourceType type, int ttl) : base(domain, type, ttl)
        {
        }

        /// <summary>
        /// Gets or sets the target this resource points to.
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// Returns a byte[] representation of this Resource.
        /// </summary>
        /// <returns>A byte[] representation of this Resource.</returns>
        protected override byte[] GetBytesInternal()
        {
            var packetBuilder = new PacketBuilder();
            packetBuilder.Add(Target);
            return packetBuilder.GetBytes();
        }
    }
}
