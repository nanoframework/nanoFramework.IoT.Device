// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.MulticastDns.Enum;

namespace Iot.Device.MulticastDns.EventArgs
{
    /// <summary>
    /// The EventArgs used to pass the status of the service.
    /// </summary>
    public class MulticastDnsStatusEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MulticastDnsStatusEventArgs" /> class.
        /// </summary>
        /// <param name="status">The communicated status of the service.</param>
        /// <param name="message">The optional message accompanying the status.</param>
        public MulticastDnsStatusEventArgs(MulticastDnsStatus status, string message = "")
        {
            Status = status;
            Message = message;
        }

        /// <summary>
        /// Gets the communicated status of the service.
        /// </summary>
        public MulticastDnsStatus Status { get; }

        /// <summary>
        /// Gets the optional message accompanying the status.
        /// </summary>
        public string Message { get; }
    }
}
