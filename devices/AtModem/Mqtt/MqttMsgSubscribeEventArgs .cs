// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace nanoFramework.M2Mqtt.Messages
{
    /// <summary>
    /// Event Args class for subscribe request on topics
    /// </summary>
    public class MqttMsgSubscribeEventArgs : EventArgs
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
        /// List of QOS Levels requested
        /// </summary>
        public MqttQoSLevel[] QoSLevels { get; internal set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageId">Message identifier for subscribe topics request</param>
        /// <param name="topics">Topics requested to subscribe</param>
        /// <param name="qosLevels">List of QOS Levels requested</param>
        public MqttMsgSubscribeEventArgs(ushort messageId, string[] topics, MqttQoSLevel[] qosLevels)
        {
            MessageId = messageId;
            Topics = topics;
            QoSLevels = qosLevels;
        }
    }
}