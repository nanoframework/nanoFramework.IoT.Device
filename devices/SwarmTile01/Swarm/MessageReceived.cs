//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;

namespace Iot.Device.Swarm
{
    /// <summary>
    /// Message received from the Swarm network.
    /// </summary>
    public class MessageReceived : MessageBase
    {
        /// <summary>
        /// Message identifier assigned by the device.
        /// </summary>
        /// <remarks>
        /// This property is empty for messages that have been created by code.
        /// </remarks>
        public string ID { get; internal set; }

        /// <summary>
        /// Time-stamp of the moment the message was received by the Tile.
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
