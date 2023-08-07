// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Net.Security
{
    /// <summary>
    /// Defines the possible versions of Secure Sockets Layer (SSL).
    /// </summary>
    /// <remarks>
    /// Note: Following the recommendation of the .NET documentation, nanoFramework implementation does not have SSL3 nor Default because those are deprecated and unsecure.
    /// </remarks>
    [FlagsAttribute]
    public enum SslProtocols
    {
        /// <summary>
        /// Allows the operating system to choose the best protocol to use, and to block protocols that are not secure. Unless your app has a specific reason not to, you should use this field.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Specifies the TLS 1.0 security protocol.
        /// The TLS protocol is defined in IETF RFC 2246.
        /// </summary>
        Tls = 0x10,

        /// <summary>
        /// Specifies the TLS 1.1 security protocol.
        /// The TLS protocol is defined in IETF RFC 4346.
        /// </summary>
        Tls11 = 0x20,

        /// <summary>
        /// Specifies the TLS 1.2 security protocol.
        /// The TLS protocol is defined in IETF RFC 5246.
        /// </summary>
        Tls12 = 0x40,
    }
}