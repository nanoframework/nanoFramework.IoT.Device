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
    /// Represents the event arguments for successfully subscribed topics.
    /// </summary>
    public class MqttMsgSubscribedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the message identifier associated with the subscription request.
        /// </summary>
        public ushort MessageId { get; internal set; }

        /// <summary>
        /// Gets the list of granted Quality of Service (QoS) levels for the subscribed topics.
        /// </summary>
        public MqttQoSLevel[] GrantedQoSLevels { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MqttMsgSubscribedEventArgs"/> class with the specified parameters.
        /// </summary>
        /// <param name="messageId">The message identifier associated with the subscription request.</param>
        /// <param name="grantedQosLevels">The list of granted Quality of Service (QoS) levels for the subscribed topics.</param>
        public MqttMsgSubscribedEventArgs(ushort messageId, MqttQoSLevel[] grantedQosLevels)
        {
            MessageId = messageId;
            GrantedQoSLevels = grantedQosLevels;
        }
    }
}
