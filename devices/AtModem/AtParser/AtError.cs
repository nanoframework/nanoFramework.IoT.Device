// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace IoT.Device.AtModem
{
    /// <summary>
    /// Represents the possible errors in AT commands.
    /// </summary>
    public enum AtError
    {
        /// <summary>
        /// No error occurred.
        /// </summary>
        NoError,

        /// <summary>
        /// Invalid response received.
        /// </summary>
        InvalidResponse,

        /// <summary>
        /// Command is still pending.
        /// </summary>
        CommandPending,

        /// <summary>
        /// Timeout occurred while waiting for a response.
        /// </summary>
        Timeout,

        /// <summary>
        /// The channel is closed.
        /// </summary>
        ChannelClosed,
    }
}
