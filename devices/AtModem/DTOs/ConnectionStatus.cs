// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.AtModem.DTOs
{
    /// <summary>
    /// Information about the status of the connection.
    /// </summary>
    public enum ConnectionStatus
    {
        /// <summary>
        /// The connection is disconnected.
        /// </summary>
        Disconnected,

        /// <summary>
        /// The connection is connected.
        /// </summary>
        Connected,

        /// <summary>
        /// An error occured when establishing a connection.
        /// </summary>
        Error
    }
}
