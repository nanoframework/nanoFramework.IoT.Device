// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.MulticastDNS.Enum;
using Iot.Device.MulticastDNS.Package;

namespace Iot.Device.MulticastDNS.Entities
{
    /// <summary>
    /// A Resource is part of a Message and can be an Answer, a Server or an Additional Resource.
    /// </summary>
    public class Resource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Resource" /> class.
        /// </summary>
        /// <param name="domain">The domain this resource is about.</param>
        /// <param name="type">The type of this resource.</param>
        /// <param name="ttl">The TTL of this resource.</param>
        public Resource(string domain, DnsResourceType type, int ttl) : this(domain, ttl)
            => ResourceType = type;

        /// <summary>
        /// Initializes a new instance of the <see cref="Resource" /> class.
        /// </summary>
        /// <param name="domain">The domain this resource is about.</param>
        /// <param name="ttl">The TTL of this resource.</param>
        public Resource(string domain, int ttl)
        {
            Domain = domain;
            Ttl = ttl;
        }

        /// <summary>
        /// Gets The domain this resource is about.
        /// </summary>
        public string Domain { get; }

        /// <summary>
        /// Gets The type of this resource.
        /// </summary>
        public DnsResourceType ResourceType { get; }

        /// <summary>
        /// Gets The class of this resource.
        /// </summary>
        public ushort ResourceClass { get; } = 1; // IN

        /// <summary>
        /// Gets or sets The TTL of this resource.
        /// </summary>
        public int Ttl { get; set; } = 2000;

        /// <summary>
        /// Returns a byte[] representation of this Resource.
        /// </summary>
        /// <returns>A byte[] representation of this Resource.</returns>
        public byte[] GetBytes()
        {
            PacketBuilder packet = new PacketBuilder();
            packet.Add(Domain);
            packet.Add((ushort)ResourceType);
            packet.Add(ResourceClass);
            packet.Add(Ttl);

            var data = GetBytesInternal();

            packet.Add((ushort)data.Length);
            packet.Add(data);

            return packet.GetBytes();
        }

        /// <summary>
        /// Returns a byte[] representation of this Resource.
        /// </summary>
        /// <returns>A byte[] representation of this Resource.</returns>
        protected virtual byte[] GetBytesInternal() => new byte[0]; 
    }
}
