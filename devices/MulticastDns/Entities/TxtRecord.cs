// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Text;
using Iot.Device.MulticastDns.Enum;
using Iot.Device.MulticastDns.Package;

namespace Iot.Device.MulticastDns.Entities
{
    /// <summary>
    /// Represents a TXT Record Resource (DNS Resource Type 16).
    /// </summary>
    public class TxtRecord : Resource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TxtRecord" /> class.
        /// </summary>
        /// <param name="domain">The domain this Record is about.</param>
        /// <param name="txt">The text this resource represents.</param>
        /// <param name="ttl">The TTL of this SRVRecord.</param>
        public TxtRecord(string domain, string txt, int ttl = 2000) : base(domain, DnsResourceType.TXT, ttl)
        {
            if (Encoding.UTF8.GetBytes(txt).Length > 255)
            {
                throw new ArgumentException($"TXT record value exceeds maximum encoded length of 255 bytes.", nameof(txt));
            }

            Txt = txt;
        }

        internal TxtRecord(PacketParser packet, string domain, int ttl, ushort rrClass) : base(domain, DnsResourceType.TXT, ttl, rrClass)
            => Txt = packet.ReadString();

        /// <summary>
        /// Gets the text this resource represents.
        /// </summary>
        public string Txt { get; }

        /// <summary>
        /// Returns a byte[] representation of this Resource.
        /// </summary>
        /// <returns>A byte[] representation of this Resource.</returns>
        protected override byte[] GetBytesInternal()
        {
            var txtBytes = Encoding.UTF8.GetBytes(Txt);
            var result = new byte[1 + txtBytes.Length];
            result[0] = (byte)txtBytes.Length;
            Array.Copy(txtBytes, 0, result, 1, txtBytes.Length);
            return result;
        }
    }
}
