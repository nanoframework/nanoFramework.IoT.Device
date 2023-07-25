// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace IoT.Device.AtModem.DTOs
{
    /// <summary>
    /// Enumerates the network registration status for a mobile device.
    /// </summary>
    public enum NetworkRegistration
    {
        /// <summary>
        /// The device is not registered on any network.
        /// </summary>
        NotRegistered = 0,

        /// <summary>
        /// The device is registered on its home network.
        /// </summary>
        RegisteredHomeNetwork = 1,

        /// <summary>
        /// The device is not registered on any network and is searching for available networks.
        /// </summary>
        NotRegisteredSearching = 2,

        /// <summary>
        /// The registration request of the device is denied by the network.
        /// </summary>
        RegistrationDenied = 3,

        /// <summary>
        /// The network registration status is unknown.
        /// </summary>
        Unknown = 4,

        /// <summary>
        /// The device is registered on a roaming network.
        /// </summary>
        RegisteredRoaming = 5,
    }
}
