// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace IoT.Device.AtModem.DTOs
{
    /// <summary>
    /// Network Information. 
    /// </summary>
    public class NetworkInformation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkInformation"/> class. 
        /// </summary>
        public NetworkInformation()
        {
        }

        /// <summary>
        /// Gets or sets the name of the Network Operator.
        /// </summary>
        public string NetworkOperator { get; set; }

        /// <summary>
        /// Gets or sets  the Signal Strength.
        /// </summary>
        public SignalStrength SignalQuality { get; set; }

        /// <summary>
        /// Gets or sets the IP Address assigned.
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// Gets or sets the cellular network connection status.
        /// </summary>
        public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
    }
}
