// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using nanoFramework.M2Mqtt.Messages;

namespace nanoFramework.M2Mqtt
{
    /// <summary>
    /// Interface for MQTT client.
    /// </summary>
    /// <remarks>This is compatible with the full nanoFramceork M2Mqtt client. But the implementation is different.
    /// The change of the nanoFramework M2Mqtt client is not done yet. Once done, this interface will be removed and replaced by a reference to the core nanoFramework M2Mqtt client nuget containing the core elements.
    /// In the longer term, this will allow to use higher level classes like Azure or AWS in a full transparent way.
    /// </remarks>
    public partial class MqttClientBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="brokerHostName">Broker Host Name or IP Address</param>
        /// <param name="brokerPort">Broker port</param>
        /// <param name="secure">Using secure connection</param>
        /// <param name="caCert">CA certificate for secure connection</param>
        /// <param name="clientCert">Client certificate</param>
        /// <param name="sslProtocol">SSL/TLS protocol</param>
        public MqttClientBase(string brokerHostName, int brokerPort, bool secure, X509Certificate caCert, X509Certificate clientCert, MqttSslProtocols sslProtocol)
        {
            BrokerHostName = brokerHostName;
            BrokerPort = brokerPort;
            Secure = secure;
        }

        /// <summary>
        /// Gets a value indicating whether the connection status between client and broker.
        /// </summary>
        public bool IsConnected { get; internal set; }

        internal string ClientId { get; set; }

        internal string Username { get; set; }

        internal string Password { get; set; }

        internal string BrokerHostName { get; set; }

        internal int BrokerPort { get; set; }

        internal bool Secure { get; set; }


        /// <summary>
        /// Connect to broker.
        /// </summary>
        /// <param name="clientId">Client identifier.</param>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        /// <param name="willRetain">Will retain flag.</param>
        /// <param name="willQosLevel">Will QOS level.</param>
        /// <param name="willFlag">Will flag.</param>
        /// <param name="willTopic">Will topic.</param>
        /// <param name="willMessage">Will message.</param>
        /// <param name="cleanSession">Clean sessione flag.</param>
        /// <param name="keepAlivePeriod">Keep alive period.</param>
        /// <returns>Return code of CONNACK message from broker.</returns>
        public MqttReasonCode Connect(
            string clientId,
            string username,
            string password,
            bool willRetain,
            MqttQoSLevel willQosLevel,
            bool willFlag,
            string willTopic,
            string willMessage,
            bool cleanSession,
            ushort keepAlivePeriod)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Disconnect from broker.
        /// </summary>
        public void Disconnect()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Subscribe for message topics.
        /// </summary>
        /// <param name="topics">List of topics to subscribe.</param>
        /// <param name="qosLevels">QOS levels related to topics.</param>
        /// <returns>Message Id related to SUBSCRIBE message.</returns>
        public ushort Subscribe(string[] topics, MqttQoSLevel[] qosLevels)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Unsubscribe for message topics.
        /// </summary>
        /// <param name="topics">List of topics to unsubscribe.</param>
        /// <returns>Message Id in UNSUBACK message from broker.</returns>
        public ushort Unsubscribe(string[] topics)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Publish a message asynchronously.
        /// </summary>
        /// <param name="topic">Message topic.</param>
        /// <param name="message">Message data (payload).</param>
        /// <param name="contentType">Content of the application message. This is only available for MQTT v5.0.</param>
        /// <param name="userProperties">User properties for the application message. This is only available for MQTT v5.0.</param>
        /// <param name="qosLevel">QoS Level.</param>
        /// <param name="retain">Retain flag.</param>
        /// <returns>Message Id related to PUBLISH message.</returns>
        /// <exception cref="ArgumentException">If <paramref name="userProperties"/> elements aren't of type <see cref="UserProperty"/>.</exception>
        /// <exception cref="NotSupportedException">If setting a parameter that is not supported in the MQTT version set for this <see cref="MqttClientBase"/>.</exception>
        public ushort Publish(
            string topic,
            byte[] message,
            string contentType,
            ArrayList userProperties,
            MqttQoSLevel qosLevel,
            bool retain)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Delegate that defines event handler for PUBLISH message received.
        /// </summary>
        public delegate void MqttMsgPublishEventHandler(object sender, MqttMsgPublishEventArgs e);

        /// <summary>
        /// The event for PUBLISH message received.
        /// </summary>
        public event MqttMsgPublishEventHandler MqttMsgPublishReceived;

        /// <summary>
        /// Delegate that defines event handler for published message.
        /// </summary>
        public delegate void MqttMsgPublishedEventHandler(object sender, MqttMsgPublishedEventArgs e);

        /// <summary>
        /// The event for published message.
        /// </summary>
        public event MqttMsgPublishedEventHandler MqttMsgPublished;

        /// <summary>
        /// Delegate that defines event handler for subscribed topic.
        /// </summary>
        public delegate void MqttMsgSubscribedEventHandler(object sender, MqttMsgSubscribedEventArgs e);

        /// <summary>
        /// The event for subscribed topic.
        /// </summary>
        public event MqttMsgSubscribedEventHandler MqttMsgSubscribed;

        /// <summary>
        /// Delegate that defines event handler for unsubscribed topic.
        /// </summary>
        public delegate void MqttMsgUnsubscribedEventHandler(object sender, MqttMsgUnsubscribedEventArgs e);

        /// <summary>
        /// The event for unsubscribed topic.
        /// </summary>
        public event MqttMsgUnsubscribedEventHandler MqttMsgUnsubscribed;

        /// <summary>
        /// Delegate that defines event handler for client/peer disconnection.
        /// </summary>
        public delegate void ConnectionClosedEventHandler(object sender, EventArgs e);

        /// <summary>
        /// The event for peer/client disconnection.
        /// </summary>
        public event ConnectionClosedEventHandler ConnectionClosed;
    }
}
