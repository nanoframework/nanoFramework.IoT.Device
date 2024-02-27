// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.AtModem.DTOs;

namespace Iot.Device.AtModem.Events
{
    /// <summary>
    /// Network connection event arguments.
    /// </summary>
    public class NetworkConnectionEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the network registration status.
        /// </summary>
        public NetworkRegistration NetworkRegistration { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkConnectionEventArgs"/> class with the specified network registration.
        /// </summary>
        /// <param name="networkRegistration">The network registration status.</param>
        public NetworkConnectionEventArgs(NetworkRegistration networkRegistration)
        {
            NetworkRegistration = networkRegistration;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="NetworkConnectionEventArgs"/> class from the response string.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>A new instance of the <see cref="NetworkConnectionEventArgs"/>.</returns>
        public static NetworkConnectionEventArgs CreateFromResponse(string response)
        {
            // +CREG: <stat>[,<lac>,<ci>[,<AcT>]]
            string[] split = response.Substring(7).Split(',');
            return new NetworkConnectionEventArgs((NetworkRegistration)int.Parse(split[0]));
        }
    }
}
