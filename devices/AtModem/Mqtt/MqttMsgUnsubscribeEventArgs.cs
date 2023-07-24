// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace nanoFramework.M2Mqtt.Messages
{
    /// <summary>
    /// Event Args class for unsubscribe request on topics
    /// </summary>
    public class MqttMsgUnsubscribeEventArgs : EventArgs
    {
        /// <summary>
        /// Message identifier
        /// </summary>
        public ushort MessageId { get; internal set; }

        /// <summary>
        /// Topics requested to subscribe
        /// </summary>
        public string[] Topics { get; internal set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageId">Message identifier for subscribed topics</param>
        /// <param name="topics">Topics requested to subscribe</param>
        public MqttMsgUnsubscribeEventArgs(ushort messageId, string[] topics)
        {
            MessageId = messageId;
            Topics = topics;
        }
    }
}