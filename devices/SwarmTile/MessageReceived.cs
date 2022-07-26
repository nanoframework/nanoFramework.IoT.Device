// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.

using System;

namespace Iot.Device.Swarm
{
    /// <summary>
    /// Message received from the Swarm network.
    /// </summary>
    public class MessageReceived : MessageBase
    {
        /// <summary>
        /// Gets message identifier assigned by the device.
        /// </summary>
        /// <remarks>
        /// This property is empty for messages that have been created by code.
        /// </remarks>
        public string ID { get; internal set; }

        /// <summary>
        /// Gets time-stamp of the moment the message was received by the Tile.
        /// </summary>
        /// <remarks>
        /// This property is empty for messages that have been created by code.
        /// </remarks>
        public DateTime TimeStamp { get; internal set; }

        /// <inheritdoc/>
        public MessageReceived(string data, uint applicationId = 0) : base(data, applicationId)
        {
        }

        /// <inheritdoc/>
        public MessageReceived(byte[] data, uint applicationId = 0) : base(data, applicationId)
        {
        }
    }
}
