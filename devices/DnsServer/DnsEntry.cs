// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Net;

namespace Iot.Device.DnsServer
{
    /// <summary>
    /// Represents a DNS entry.
    /// </summary>
    public class DnsEntry
    {
        /// <summary>
        /// Name field of the DNS entry to match against the question.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// IP address associated with the DNS entry.
        /// </summary>
        public IPAddress Address { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="DnsEntry"/> class.
        /// </summary>
        /// <param name="name">Name field of the DNS entry to match against the question.</param>
        /// <param name="address">IP address associated with the DNS entry.</param>
        public DnsEntry(
            string name,
            IPAddress address)
        {
            Name = name ?? throw new ArgumentNullException();
            Address = address;
        }
    }
}
