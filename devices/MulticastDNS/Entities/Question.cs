// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.MulticastDNS.Enum;
using Iot.Device.MulticastDNS.Package;

namespace Iot.Device.MulticastDNS.Entities
{
    /// <summary>
    /// A Multicast DNS message potentially contains Questions.
    /// </summary>
    public class Question
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Question" /> class.
        /// </summary>
        /// <param name="domain">The domain this question is about.</param>
        /// <param name="queryType">The Type of DNS Resource being queried.</param>
        /// <param name="queryClass">The class of Query this Question is about.</param>
        public Question(string domain, DnsResourceType queryType, ushort queryClass)
        {
            Domain = domain;
            QueryType = queryType;
            QueryClass = queryClass;
        }

        /// <summary>
        /// Gets the domain this question is about.
        /// </summary>
        public string Domain { get; }

        /// <summary>
        /// Gets the Type of DNS Resource being queried.
        /// </summary>
        public DnsResourceType QueryType { get; }

        /// <summary>
        /// Gets the class of Query this Question is about.
        /// </summary>
        public ushort QueryClass { get; }

        /// <summary>
        /// Returns a byte[] representation of this Question.
        /// </summary>
        /// <returns>A byte[] representation of this Question.</returns>
        public byte[] GetBytes()
        {
            PacketBuilder packet = new PacketBuilder();
            packet.Add(Domain);
            packet.Add((ushort)QueryType);
            packet.Add(QueryClass);
            return packet.GetBytes();
        }
    }
}
