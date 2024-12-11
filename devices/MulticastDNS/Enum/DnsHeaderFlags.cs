// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.MulticastDNS.Enum
{
    /// <summary>
    /// The DNS header flags are a 16 bit value according to RFC1035 section 4.1.1.
    /// </summary>
    /// <remarks>
    /// The bits are ordened as following with bith 0 indicating a query or a response:
    /// 
    ///   0  1  2  3  4  5  6  7  8  9  0  1  2  3  4  5
    /// +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    /// |QR|   Opcode  |AA|TC|RD|RA|   Z    |   RCODE   |
    /// +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    ///
    /// See https://www.rfc-editor.org/rfc/rfc1035.html#section-4.1.1 for full explanation.
    /// </remarks>
    [Flags]
    public enum DnsHeaderFlags : ushort
    {
        /// <summary>
        /// Indicates a DNS query
        /// </summary>
        Query = 0x0,

        /// <summary>
        /// Indicates a successful response
        /// </summary>
        Response = 0x8000
    }
}
