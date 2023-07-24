// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace nanoFramework.M2Mqtt.Messages
{
    /// <summary>
    /// Event Args class for unsubscribed topic
    /// </summary>
    public class MqttMsgUnsubscribedEventArgs : EventArgs
    {
        /// <summary>
        /// Message identifier
        /// </summary>
        public ushort MessageId { get; internal set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageId">Message identifier for unsubscribed topic</param>
        public MqttMsgUnsubscribedEventArgs(ushort messageId)
        {
            MessageId = messageId;
        }
    }
}