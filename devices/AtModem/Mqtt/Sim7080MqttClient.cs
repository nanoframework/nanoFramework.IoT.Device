// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using IoT.Device.AtModem.Modem;
using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;

namespace IoT.Device.AtModem.Mqtt
{
    /// <summary>
    /// MQTT Client for SIM7080.
    /// </summary>
    public class Sim7080MqttClient : IMqttClient
    {
        private ModemBase _modem;

        internal Sim7080MqttClient(ModemBase modem)
        {
            _modem = modem;
        }

        /// <inheritdoc/>
        public bool IsConnected => throw new System.NotImplementedException();

        /// <inheritdoc/>
        public event IMqttClient.MqttMsgPublishEventHandler MqttMsgPublishReceived;

        /// <inheritdoc/>
        public event IMqttClient.MqttMsgPublishedEventHandler MqttMsgPublished;

        /// <inheritdoc/>
        public event IMqttClient.MqttMsgSubscribedEventHandler MqttMsgSubscribed;

        /// <inheritdoc/>
        public event IMqttClient.MqttMsgUnsubscribedEventHandler MqttMsgUnsubscribed;

        /// <inheritdoc/>
        public event IMqttClient.ConnectionClosedEventHandler ConnectionClosed;

        /// <inheritdoc/>
        public MqttReasonCode Connect(string clientId, string username, string password, bool willRetain, MqttQoSLevel willQosLevel, bool willFlag, string willTopic, string willMessage, bool cleanSession, ushort keepAlivePeriod)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public void Disconnect()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public ushort Publish(string topic, byte[] message, string contentType, ArrayList userProperties, MqttQoSLevel qosLevel, bool retain)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public ushort Subscribe(string[] topics, MqttQoSLevel[] qosLevels)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public ushort Unsubscribe(string[] topics)
        {
            throw new System.NotImplementedException();
        }
    }
}
