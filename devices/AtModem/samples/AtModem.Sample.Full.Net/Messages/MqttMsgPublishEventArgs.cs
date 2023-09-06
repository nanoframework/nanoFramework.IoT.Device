/*
Copyright (c) 2013, 2014 Paolo Patierno

All rights reserved. This program and the accompanying materials
are made available under the terms of the Eclipse Public License v1.0
and Eclipse Distribution License v1.0 which accompany this distribution. 

The Eclipse Public License is available at 
   http://www.eclipse.org/legal/epl-v10.html
and the Eclipse Distribution License is available at 
   http://www.eclipse.org/org/documents/edl-v10.php.

Contributors:
   Paolo Patierno - initial API and implementation and/or initial documentation
   .NET Foundation and Contributors - nanoFramework support
*/

using System;

namespace nanoFramework.M2Mqtt.Messages
{
    /// <summary>
    /// Represents the event arguments for a PUBLISH message received from the broker.
    /// </summary>
    public class MqttMsgPublishEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the topic of the received message.
        /// </summary>
        public string Topic { get; internal set; }

        /// <summary>
        /// Gets the data of the received message.
        /// </summary>
        public byte[] Message { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the message is a duplicate delivery.
        /// </summary>
        public bool DupFlag { get; internal set; }

        /// <summary>
        /// Gets the Quality of Service (QoS) level of the message.
        /// </summary>
        public MqttQoSLevel QosLevel { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the message should be retained by the broker.
        /// </summary>
        public bool Retain { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MqttMsgPublishEventArgs"/> class with the specified parameters.
        /// </summary>
        /// <param name="topic">The topic of the received message.</param>
        /// <param name="message">The data of the received message.</param>
        /// <param name="dupFlag">A value indicating whether the message is a duplicate delivery.</param>
        /// <param name="qosLevel">The Quality of Service (QoS) level of the message.</param>
        /// <param name="retain">A value indicating whether the message should be retained by the broker.</param>
        public MqttMsgPublishEventArgs(string topic, byte[] message, bool dupFlag, MqttQoSLevel qosLevel, bool retain)
        {
            Topic = topic;
            Message = message;
            DupFlag = dupFlag;
            QosLevel = qosLevel;
            Retain = retain;
        }
    }
}
