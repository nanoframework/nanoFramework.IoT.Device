// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.IO.Ports;
using System.Text;
using IoT.Device.AtModem.Events;
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
        private const string CaCertName = "ca.crt";
        private const string ClientCertName = "client.crt";
        private ModemBase _modem;
        private string _brokerHostName;
        private int _brokerPort;
        private bool _secure;
        private ushort _messageId = 0;

        private MqttSslProtocols _sslProtocol;

        internal Sim7080MqttClient(ModemBase modem)
        {
            _modem = modem;
            _modem.GenericEvent += ModemGenericEvent;
        }

        private void ModemGenericEvent(object sender, GenericEventArgs e)
        {
            if (e.Message.StartsWith("+SMSUB: "))
            {
                var line = e.Message.Substring(8);
                var elements = line.Split(',');
                var topic = elements[0].Trim('"');
                var message = elements[1].Trim('"');

                // There is no trace of QoS Level or retain flag in the response
                MqttMsgPublishReceived?.Invoke(sender, new MqttMsgPublishEventArgs(topic, Encoding.UTF8.GetBytes(message), false, MqttQoSLevel.AtMostOnce, false));
            }
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
        public void Init(string brokerHostName, int brokerPort, bool secure, byte[] caCert, byte[] clientCert, MqttSslProtocols sslProtocol)
        {
            _brokerHostName = brokerHostName;
            _brokerPort = brokerPort;
            _secure = secure;
            _sslProtocol = sslProtocol;

            // Store the caCert and the client cert in the storage
            _modem.FileStorage.WriteFile(CaCertName, caCert);
            _modem.FileStorage.WriteFile(ClientCertName, clientCert);

            // Set the SSL parameters, this is using the index 1, that may have to be updated somewhow
            var response = _modem.Channel.SendCommand($"AT+SMSSL=1,\"{CaCertName}\",\"{ClientCertName}\"");
        }

        /// <inheritdoc/>
        public MqttReasonCode Connect(string clientId, string username, string password, bool willRetain, MqttQoSLevel willQosLevel, bool willFlag, string willTopic, string willMessage, bool cleanSession, ushort keepAlivePeriod)
        {
            // Server URL and port
            var response = _modem.Channel.SendCommand($"AT+SMCONF=\"URL\",\"{_brokerHostName}\",\"{_brokerPort}\"");
            if (!response.Success)
            {
                return MqttReasonCode.UnspecifiedError;
            }

            // Set MQTT time to connect server
            response = _modem.Channel.SendCommand("AT+SMCONF=\"KEEPTIME\",60");
            if (!response.Success)
            {
                return MqttReasonCode.UnspecifiedError;
            }

            // Setup clean session
            response = _modem.Channel.SendCommand($"AT+SMCONF=\"CLEANSS\",{(cleanSession ? "1" : "0")}");
            if (!response.Success)
            {
                return MqttReasonCode.UnspecifiedError;
            }

            // Setup the QOS level
            response = _modem.Channel.SendCommand($"AT+SMCONF=\"QOS\",{(int)willQosLevel}");
            if (!response.Success)
            {
                return MqttReasonCode.UnspecifiedError;
            }

            // Setup the will flag
            response = _modem.Channel.SendCommand($"AT+SMCONF=\"RETAIN\",{(willFlag ? "1" : "0")}");
            if (!response.Success)
            {
                return MqttReasonCode.UnspecifiedError;
            }

            // Setup the will measse
            response = _modem.Channel.SendCommand($"AT+SMCONF=\"MESSAGE\",\"{willMessage}\"");
            if (!response.Success)
            {
                return MqttReasonCode.UnspecifiedError;
            }

            // Simcom module MQTT parameter that sets the client id
            response = _modem.Channel.SendCommand($"AT+SMCONF=\"CLIENTID\",\"{clientId}\"");
            if (!response.Success)
            {
                return MqttReasonCode.UnspecifiedError;
            }

            // Simcom module MQTT parameter that sets the user name
            response = _modem.Channel.SendCommand($"AT+SMCONF=\"USERNAME\",\"{username}\"");
            if (!response.Success)
            {
                return MqttReasonCode.UnspecifiedError;
            }

            // Simcom module MQTT parameter that sets the secure access token
            response = _modem.Channel.SendCommand($"AT+SMCONF=\"PASSWORD\",\"{password}\"");
            if (!response.Success)
            {
                return MqttReasonCode.UnspecifiedError;
            }

            // Setup the will topic
            response = _modem.Channel.SendCommand($"AT+SMCONF=\"\"TOPIC\"\",\"{willTopic}\"");
            if (!response.Success)
            {
                return MqttReasonCode.UnspecifiedError;
            }

            // Simcom module MQTT open the connection
            response = _modem.Channel.SendCommand("AT+SMCONN", TimeSpan.FromMinutes(1));
            if (!response.Success)
            {
                return MqttReasonCode.UnspecifiedError;
            }

            return MqttReasonCode.Success;
        }

        /// <inheritdoc/>
        public void Disconnect()
        {
            // Disconnect from the MQTT server
            _modem.Channel.SendCommand("AT+SMDISC");
            ConnectionClosed?.Invoke(this, new EventArgs());
        }

        /// <inheritdoc/>
        public ushort Publish(string topic, byte[] message, string contentType, ArrayList userProperties, MqttQoSLevel qosLevel, bool retain)
        {
            if (!IsStillConnected())
            {
                return 0;
            }

            // Publish a message on a topic
            _modem.Channel.SendBytesWithoutAck(Encoding.UTF8.GetBytes($"AT+SMPUB=\"{topic}\",{message.Length},{(int)qosLevel},{(retain ? "1" : "0")}\r"));
            _modem.Channel.SendBytesWithoutAck(message);

            // Just to make sure we have back a OK
            _modem.Channel.SendCommand("AT");
            return IncrementtMessageId();
        }

        /// <inheritdoc/>
        public ushort Subscribe(string[] topics, MqttQoSLevel[] qosLevels)
        {
            if (!IsStillConnected())
            {
                return 0;
            }

            if (topics.Length != qosLevels.Length)
            {
                throw new ArgumentException();
            }

            ushort msgId = _messageId;
            for (int i = 0; i < topics.Length; i++)
            {
                _modem.Channel.SendCommand($"AT+SMSUB=\"{topics[i]}\",{(int)qosLevels[i]}");
                msgId = IncrementtMessageId();
            }

            return msgId;
        }

        /// <inheritdoc/>
        public ushort Unsubscribe(string[] topics)
        {
            if (!IsStillConnected())
            {
                return 0;
            }

            ushort msgId = _messageId;
            foreach (var topic in topics)
            {
                _modem.Channel.SendCommand($"AT+SMUNSUB=\"{topic}\"");
                msgId = IncrementtMessageId();
            }

            return msgId;
        }

        private ushort IncrementtMessageId()
        {
            return _messageId++ == ushort.MaxValue ? _messageId = 1 : _messageId;
        }

        private bool IsStillConnected()
        {
            int state = 0;
            var response = _modem.Channel.SendSingleLineCommandAsync("AT+SMSTATE?", "+SMSTATE");
            if (response.Success)
            {
                string line = response.Intermediates.Count > 1 ? (string)response.Intermediates[1] : string.Empty;
                if (line.StartsWith("+SMSTATE: "))
                {
                    state = int.Parse(line.Substring(10));
                }
            }

            if (state == 0)
            {
                ConnectionClosed?.Invoke(this, new EventArgs());
            }

            return state != 0;
        }
    }
}
