// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.AtModem.Events
{
    /// <summary>
    /// Provides event arguments containing information about the network connection status for an application.
    /// </summary>
    public class ApplicationNetworkEventArgs : EventArgs
    {
        /// <summary>
        /// Gets a value indicating whether the application is connected to the network.
        /// </summary>
        public bool IsConnected { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationNetworkEventArgs"/> class with the specified network connection status.
        /// </summary>
        /// <param name="isConnected">A boolean value indicating whether the application is connected to the network.</param>
        public ApplicationNetworkEventArgs(bool isConnected)
        {
            IsConnected = isConnected;
        }
    }
}
