// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace nanoFramework.M2Mqtt.Messages
{
    /// <summary>
    /// Event Args class for subscribed topics
    /// </summary>
    public class MqttMsgSubscribedEventArgs : EventArgs
    {
        /// <summary>
        /// Message identifier
        /// </summary>
        public ushort MessageId { get; internal set; }

        /// <summary>
        /// List of granted QOS Levels
        /// </summary>
        public MqttQoSLevel[] GrantedQoSLevels { get; internal set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageId">Message identifier for subscribed topics</param>
        /// <param name="grantedQosLevels">List of granted QOS Levels</param>
        public MqttMsgSubscribedEventArgs(ushort messageId, MqttQoSLevel[] grantedQosLevels)
        {
            MessageId = messageId;
            GrantedQoSLevels = grantedQosLevels;
        }
    }
}