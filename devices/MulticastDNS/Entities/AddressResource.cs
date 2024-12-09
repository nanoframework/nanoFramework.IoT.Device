// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using Iot.Device.MulticastDNS.Enum;

namespace Iot.Device.MulticastDNS.Entities
{
    /// <summary>
    /// The base class for Address resources.
    /// </summary>
    public abstract class AddressResource : Resource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddressResource" /> class.
        /// </summary>
        /// <param name="domain">The domain this Record points to.</param>
        /// <param name="type">The type of this resource.</param>
        /// <param name="ttl">The TTL of this resource.</param>
        public AddressResource(string domain, DnsResourceType type, int ttl) : base(domain, type, ttl)
        {
        }

        /// <summary>
        /// Gets or sets the address that points to the domain.
        /// </summary>
        public IPAddress Address { get; set; }

        /// <summary>
        /// Returns a byte[] representation of this Resource.
        /// </summary>
        /// <returns>A byte[] representation of this Resource.</returns>
        protected override byte[] GetBytesInternal() => Address.GetAddressBytes();
    }
}
