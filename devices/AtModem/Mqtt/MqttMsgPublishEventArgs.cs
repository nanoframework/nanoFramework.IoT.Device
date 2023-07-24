// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace nanoFramework.M2Mqtt.Messages
{
    /// <summary>
    /// Event Args class for PUBLISH message received from broker
    /// </summary>
    public class MqttMsgPublishEventArgs : EventArgs
    {
        /// <summary>
        /// Message topic
        /// </summary>
        public string Topic { get; internal set; }

        /// <summary>
        /// Message data
        /// </summary>
        public byte[] Message { get; internal set; }

        /// <summary>
        /// Duplicate message flag
        /// </summary>
        public bool DupFlag { get; internal set; }

        /// <summary>
        /// Quality of Service level
        /// </summary>
        public MqttQoSLevel QosLevel { get; internal set; }

        /// <summary>
        /// Retain message flag
        /// </summary>
        public bool Retain { get; internal set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="topic">Message topic</param>
        /// <param name="message">Message data</param>
        /// <param name="dupFlag">Duplicate delivery flag</param>
        /// <param name="qosLevel">Quality of Service level</param>
        /// <param name="retain">Retain flag</param>
        public MqttMsgPublishEventArgs(string topic,
                byte[] message,
                bool dupFlag,
                MqttQoSLevel qosLevel,
                bool retain)
        {
            Topic = topic;
            Message = message;
            DupFlag = dupFlag;
            QosLevel = qosLevel;
            Retain = retain;
        }
    }
}