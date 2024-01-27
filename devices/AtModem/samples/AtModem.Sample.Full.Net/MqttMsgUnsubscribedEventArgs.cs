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
    /// Represents the event arguments for when a topic has been unsubscribed.
    /// </summary>
    public class MqttMsgUnsubscribedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the message identifier associated with the unsubscribed topic.
        /// </summary>
        public ushort MessageId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MqttMsgUnsubscribedEventArgs"/> class with the specified message identifier.
        /// </summary>
        /// <param name="messageId">The message identifier associated with the unsubscribed topic.</param>
        public MqttMsgUnsubscribedEventArgs(ushort messageId)
        {
            MessageId = messageId;
        }
    }
}
