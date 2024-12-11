// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.MulticastDns.Entities;

namespace Iot.Device.MulticastDns.EventArgs
{
    /// <summary>
    /// The EventArgs used to pass the information about a received message.
    /// </summary>
    public class MessageReceivedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageReceivedEventArgs" /> class.
        /// </summary>
        /// <param name="message">The received message.</param>
        public MessageReceivedEventArgs(Message message) => Message = message;

        /// <summary>
        /// Gets or sets the response the consumer of these EventArgs wants to send.
        /// </summary>
        public Response Response { get; set; }

        /// <summary>
        /// Gets the received message.
        /// </summary>
        public Message Message { get; }
    }
}
