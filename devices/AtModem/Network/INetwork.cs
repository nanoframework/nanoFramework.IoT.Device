// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.AtModem.DTOs;
using Iot.Device.AtModem.Events;
using System;

namespace Iot.Device.AtModem.Network
{
    /// <summary>
    /// Represents a network interface.
    /// </summary>
    public interface INetwork : IDisposable
    {
        /// <summary>
        /// Gets the network information.
        /// </summary>
        NetworkInformation NetworkInformation { get; }

        /// <summary>
        /// Gets a value indicating whether the device is connected to the network.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the device should automatically reconnect to the network.
        /// </summary>
        bool AutoReconnect { get; set; }

        /// <summary>
        /// Connects the device to the network using the specified system mode and enables reporting.
        /// </summary>        
        /// <param name="pin">Optional. The PIN to use for the connection. Default is null.</param>
        /// <param name="apn">Optional. The APN to use for the connection. Default is null.</param>
        /// <param name="maxRetry">Optional. The maximum number of retries. Default is 10.</param>
        /// <returns>True if the connection is successful, otherwise false.</returns>
        bool Connect(PersonalIdentificationNumber pin = null, AccessPointConfiguration apn = null, int maxRetry = 10);

        /// <summary>
        /// Reconnects the device to the network.
        /// </summary>
        /// <returns>True is the reconnection worked.</returns>
        bool Reconnect();

        /// <summary>
        /// Disconnects the device from the network.
        /// </summary>
        /// <returns>True if the disconnection is successful, otherwise false.</returns>
        bool Disconnect();

        /// <summary>
        /// Gets the available operators.
        /// </summary>
        /// <returns>An array of available operators.</returns>
        Operator[] GetOperators();

        /// <summary>
        /// Represents the method that will handle network-related events for an application.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="ApplicationNetworkEventArgs"/> object that contains event data.</param>
        public delegate void ApplicationNetworkEventHandler(object sender, ApplicationNetworkEventArgs e);

        /// <summary>
        /// Occurs when there is a network-related event for an application.
        /// </summary>
        public event ApplicationNetworkEventHandler ApplicationNetworkEvent;

        /// <summary>
        /// Represents the method that will handle an event with DateTime event data.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An instance of DateTimeEventArgs containing the event data.</param>
        public delegate void DateTimeEventHandler(object sender, DateTimeEventArgs e);

        /// <summary>
        /// Occurs when the DateTime value is changed.
        /// </summary>
        public event DateTimeEventHandler DateTimeChanged;
    }
}
