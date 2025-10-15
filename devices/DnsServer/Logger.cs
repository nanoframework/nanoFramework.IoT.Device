// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Logging;
using nanoFramework.Logging.Debug;

namespace Iot.Device.DnsServer
{
    /// <summary>
    /// Static logging utility class for DNS server components.
    /// </summary>
    internal static class Logger
    {
        /// <summary>
        /// Gets or sets the global logger instance.
        /// </summary>
        public static ILogger GlobalLogger { get; set; }
    }
}