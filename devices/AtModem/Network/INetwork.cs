// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using IoT.Device.AtModem.DTOs;

namespace IoT.Device.AtModem.Network
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
        /// Connects the device to the network using the specified system mode and enables reporting.
        /// </summary>        
        /// <param name="pin">Optional. The PIN to use for the connection. Default is null.</param>
        /// <param name="apn">Optional. The APN to use for the connection. Default is null.</param>
        /// <param name="maxRetry">Optional. The maximum number of retries. Default is 10.</param>
        /// <returns>True if the connection is successful, otherwise false.</returns>
        bool Connect(PersonalIdentificationNumber pin = null, AccessPointConfiguration apn = null, int maxRetry = 10);

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
    }
}
