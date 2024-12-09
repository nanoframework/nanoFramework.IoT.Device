// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.MulticastDNS.Enum
{
    /// <summary>
    /// The type of a DNS Resource.
    /// </summary>
    public enum DnsResourceType
    {
        /// <summary>
        /// DNS Resource Type A.
        /// </summary>
        A = 1,

        /// <summary>
        /// DNS Resource Type CNAME.
        /// </summary>
        CNAME = 5,

        /// <summary>
        /// DNS Resource Type PTR.
        /// </summary>
        PTR = 12,

        /// <summary>
        /// DNS Resource Type TXT.
        /// </summary>
        TXT = 16,

        /// <summary>
        /// DNS Resource Type AAAA.
        /// </summary>
        AAAA = 28,

        /// <summary>
        /// DNS Resource Type SRV.
        /// </summary>
        SRV = 33
    }
}
