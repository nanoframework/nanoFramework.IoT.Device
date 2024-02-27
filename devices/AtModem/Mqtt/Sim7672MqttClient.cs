// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Iot.Device.AtModem.CodingSchemes;
using Iot.Device.AtModem.Events;
using Iot.Device.AtModem.FileStorage;
using Iot.Device.AtModem.Modem;
using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;

namespace Iot.Device.AtModem.Mqtt
{
    /// <summary>
    /// MQTT Client for SIM7080.
    /// </summary>
    public class Sim7672MqttClient : IMqttClient, IDisposable
    {
        private const string CaCertName = "ca.pem";
        private const string ClientCertName = "cl.pem";
        private const int IndexSSL = 1;
        private Sim7672 _modem;
        private string _brokerHostName;
        private int _brokerPort;
        private bool _secure;
        private ushort _messageId = 0;
        private string _caCertName = string.Empty;
        private string _clCertName = string.Empty;
        private bool _willRetainFlag;
        private MqttSslProtocols _sslProtocol;
        private bool _isIncoming = false;
        private ArrayList _messages = new ArrayList();

        internal Sim7672MqttClient(ModemBase modem)
        {
            _modem = (Sim7672)modem;
            _modem.GenericEvent += ModemGenericEvent;
        }

        private void ModemGenericEvent(object sender, GenericEventArgs e)
        {
            // When receiving a topic, it looks like this:
            // +CMQTTRXSTART: 0,9,60
            // +CMQTTRXTOPIC: 0,9
            // simcommsg
            // That +CMQTTRXTOPIC can repeate if too long
            // +CMQTTRXPAYLOAD: 0,60
            // 012345678901234567890123456789012345678
            // 901234567890123456789
            // That +CMQTTRXPAYLOAD can repeat up to the end:
            // +CMQTTRXEND: 0
            if (e.Message.StartsWith("+CMQTTRXSTART: 0"))
            {
                _isIncoming = true;
                _messages.Clear();
            }
            else if (_isIncoming)
            {
                if (e.Message.StartsWith("+CMQTTRXEND: 0"))
                {
                    _isIncoming = false;

                    // Unpack everything
                    string topic = string.Empty;
                    string message = string.Empty;
                    try
                    {
                        for (int i = 0; i < _messages.Count; i++)
                        {
                            if (((string)_messages[i]).StartsWith("+CMQTTRXTOPIC: 0,"))
                            {
                                topic += _messages[++i];
                            }
                            else if (((string)_messages[i]).StartsWith("+CMQTTRXPAYLOAD: 0,"))
                            {
                                message += _messages[++i];
                            }
                            else
                            {
                                // It is a multiline message
                                message += "\r\n" + _messages[i];
                            }
                        }

                        MqttMsgPublishReceived?.Invoke(sender, new MqttMsgPublishEventArgs(topic, Encoding.UTF8.GetBytes(message), false, MqttQoSLevel.AtMostOnce, false));
                    }
                    catch
                    {
                        // Nothing, we just don't want this to break!
                    }
                }
                else
                {
                    _messages.Add(e.Message);
                }
            }
            else if (e.Message == "+CMQTTSTART: 0")
            {
                // Service is started
            }
            else if (e.Message.StartsWith("+CMQTTCONNECT: 0,"))
            {
                if (e.Message == "+CMQTTCONNECT: 0,0")
                {
                    // Service connected
                    IsConnected = true;
                }
                else if (IsConnected)
                {
                    ConnectionClosed?.Invoke(this, EventArgs.Empty);
                    IsConnected = false;
                }
            }
            else if ((e.Message == "+CMQTTDISC: 0,0") || e.Message.StartsWith("+CMQTTCONNLOST"))
            {
                // Service disconnected
                if (IsConnected)
                {
                    ConnectionClosed?.Invoke(this, EventArgs.Empty);
                    IsConnected = false;
                }
            }
            else if (e.Message.StartsWith("+CMQTTSTOP: 0") || (e.Message == "+CMQTTNONET"))
            {
                // Service stopped
                if (IsConnected)
                {
                    ConnectionClosed?.Invoke(this, EventArgs.Empty);
                    IsConnected = false;
                }
            }
        }

        /// <inheritdoc/>
        public bool IsConnected { get; internal set; }

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

        /// <summary>
        /// Initializes the MQTT client.
        /// </summary>
        /// <param name="brokerHostName">The broker host name.</param>
        /// <param name="brokerPort">The broker port.</param>
        /// <param name="secure">True if the connection is secured.</param>
        /// <param name="caCert">The root certificate.</param>
        /// <param name="clientCert">The client certificate.</param>
        /// <param name="sslProtocol">The SSL protocol to use.</param>
        public void Init(string brokerHostName, int brokerPort, bool secure, X509Certificate caCert, X509Certificate clientCert, MqttSslProtocols sslProtocol) =>
            Init(brokerHostName, brokerPort, secure, caCert?.GetRawCertData(), clientCert?.GetRawCertData(), sslProtocol);

        /// <inheritdoc/>
        public void Init(string brokerHostName, int brokerPort, bool secure, byte[] caCert, byte[] clientCert, MqttSslProtocols sslProtocol)
        {
            _brokerHostName = brokerHostName;
            _brokerPort = brokerPort;
            _secure = secure;
            _sslProtocol = sslProtocol;

            // FIXME: This is not working
            // Store the caCert and the client cert in the storage
            if (caCert != null)
            {
                int hash = HashHelper.ComputeHash(caCert);

                // Check first if the file exists already
                _caCertName = hash + CaCertName;
                _modem.SetCertificate(_caCertName, caCert);
            }

            if (clientCert != null)
            {
                int hash = HashHelper.ComputeHash(clientCert);

                // Check first if the file exists already
                _clCertName = hash + ClientCertName;
                _modem.SetCertificate(_clCertName, clientCert);
            }
        }

        /// <inheritdoc/>
        public MqttReasonCode Connect(string clientId, string username, string password, bool willRetain, MqttQoSLevel willQosLevel, bool willFlag, string willTopic, string willMessage, bool cleanSession, ushort keepAlivePeriod)
        {
            int retries = 3;
            _willRetainFlag = willRetain;

            if (_secure)
            {
                // This is to read the current configuration, we don't need it now.
                switch (_sslProtocol)
                {
                    case MqttSslProtocols.None:
                        break;
                    case MqttSslProtocols.SSLv3:
                        _modem.Channel.SendCommand($"AT+CSSLCFG=\"sslversion\",{IndexSSL},0");
                        break;
                    case MqttSslProtocols.TLSv1_0:
                        _modem.Channel.SendCommand($"AT+CSSLCFG=\"sslversion\",{IndexSSL},1");
                        break;
                    case MqttSslProtocols.TLSv1_1:
                        _modem.Channel.SendCommand($"AT+CSSLCFG=\"sslversion\",{IndexSSL},2");
                        break;
                    case MqttSslProtocols.TLSv1_2:
                        _modem.Channel.SendCommand($"AT+CSSLCFG=\"sslversion\",{IndexSSL},3");
                        break;
                    default:
                        _modem.Channel.SendCommand($"AT+CSSLCFG=\"sslversion\",{IndexSSL},4");
                        break;
                }

                // If there is a certificate attached, we check it otherwise, we ignore (less secure)
                if (_caCertName != string.Empty)
                {
                    _modem.Channel.SendCommand($"AT+CSSLCFG=\"cacert\",{IndexSSL},\"{_caCertName}\"");
                }
                else
                {
                    _modem.Channel.SendCommand($"AT+CSSLCFG=\"cacert\",{IndexSSL},\"\"");
                }

                // If there is a client certificate attached, we check it otherwise, we ignore (less secure)
                if (_clCertName != string.Empty)
                {
                    _modem.Channel.SendCommand($"AT+CSSLCFG=\"clientcert\",{IndexSSL},\"{_clCertName}\"");
                }
                else
                {
                    _modem.Channel.SendCommand($"AT+CSSLCFG=\"clientcert\",{IndexSSL},\"\"");
                }
            }

            // Just start the service, if already started, no effect.
            var response = _modem.Channel.SendCommand($"AT+CMQTTSTART", TimeSpan.FromSeconds(12));
            if (!response.Success)
            {
                Disconnect();
                Thread.Sleep(100);

                // Give it another try
                _modem.Channel.SendCommand($"AT+CMQTTSTART", TimeSpan.FromSeconds(12));
            }

            // Server URL and port
            response = _modem.Channel.SendCommand($"AT+CMQTTACCQ=0,\"{clientId}\",{(_secure ? "1" : "0")}");
            if (!response.Success)
            {
                return MqttReasonCode.UnspecifiedError;
            }

            if (_secure)
            {
                _modem.Channel.SendCommand($"AT+CMQTTSSLCFG=0,{IndexSSL}");
            }

            if (!string.IsNullOrEmpty(willTopic))
            {
                // Set the will topic
                var willBytes = Encoding.UTF8.GetBytes(willTopic);
                if (!_modem.UploadData($"AT+CMQTTWILLTOPIC=0,{willBytes.Length}", willBytes))
                {
                    return MqttReasonCode.UnspecifiedError;
                }
            }

            if (!string.IsNullOrEmpty(willMessage))
            {
                // Set the will message
                var willBytes = Encoding.UTF8.GetBytes(willMessage);
                if (!_modem.UploadData($"AT+CMQTTWILLMSG=0,{willBytes.Length},{(int)willQosLevel}", willBytes))
                {
                    return MqttReasonCode.UnspecifiedError;
                }
            }

        RetryConnect:
            // Server URL and port
            response = _modem.Channel.SendCommand($"AT+CMQTTCONNECT=0,\"tcp://{_brokerHostName}:{_brokerPort}\",60,{(cleanSession ? "1" : "0")},{(string.IsNullOrEmpty(username) ? string.Empty : $"\"{username}\"")},{(string.IsNullOrEmpty(password) ? string.Empty : $"\"{password}\"")}", TimeSpan.FromMinutes(1));
            if (!response.Success)
            {
                if (IsConnected)
                {
                    Disconnect();
                }

                if (retries-- > 0)
                {
                    Thread.Sleep(1000);
                    goto RetryConnect;
                }

                return MqttReasonCode.UnspecifiedError;
            }

            // We give maximum 5 seconds to connect, after than, we can consider it's not ok!
            CancellationTokenSource cts = new CancellationTokenSource(5000);
            while (!IsConnected || !cts.IsCancellationRequested)
            {
                Thread.Sleep(20);
            }

            return IsConnected ? MqttReasonCode.Success : MqttReasonCode.UnspecifiedError;
        }

        /// <inheritdoc/>
        public void Disconnect()
        {
            // Disconnect from the MQTT server
            _modem.Channel.SendCommand("AT+CMQTTDISC=0,60");
            Thread.Sleep(400);
            _modem.Channel.SendCommand("AT+CMQTTREL=0");
            Thread.Sleep(400);
            _modem.Channel.SendCommand("AT+CMQTTSTOP");
            if (IsConnected == true)
            {
                IsConnected = false;
                ConnectionClosed?.Invoke(this, new EventArgs());
            }
        }

        /// <inheritdoc/>
        public ushort Publish(string topic, byte[] message, string contentType, ArrayList userProperties, MqttQoSLevel qosLevel, bool retain)
        {
            // We have to send the topic and then send the message once the > prompt is here
            _modem.UploadData($"AT+CMQTTTOPIC=0,{topic.Length}", Encoding.UTF8.GetBytes(topic));
            _modem.UploadData($"AT+CMQTTPAYLOAD=0,{message.Length}", message);
            _modem.Channel.SendCommand($"AT+CMQTTPUB=0,{(int)qosLevel},60,{(retain ? "1" : "0")}");
            return IncrementtMessageId();
        }

        /// <inheritdoc/>
        public ushort Subscribe(string[] topics, MqttQoSLevel[] qosLevels)
        {
            if (!IsConnected)
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
                _modem.UploadData($"AT+CMQTTSUB=0,{topics[i].Length},{(int)qosLevels[i]}", Encoding.UTF8.GetBytes(topics[i]));
                msgId = IncrementtMessageId();
            }

            return msgId;
        }

        /// <inheritdoc/>
        public ushort Unsubscribe(string[] topics)
        {
            if (!IsConnected)
            {
                return 0;
            }

            ushort msgId = _messageId;
            foreach (var topic in topics)
            {
                _modem.UploadData($"AT+CMQTTUNSUB=0,{topic.Length}", Encoding.UTF8.GetBytes(topic));
                msgId = IncrementtMessageId();
            }

            return msgId;
        }

        private ushort IncrementtMessageId()
        {
            return _messageId++ == ushort.MaxValue ? _messageId = 1 : _messageId;
        }

        /// <inheritdoc/>
        public void Close()
        {
            Disconnect();
        }

        /// <inheritdoc/>
        public ushort Publish(
            string topic,
            byte[] message,
            string contentType)
        {
            return Publish(
                topic,
                message,
                contentType,
                null,
                MqttQoSLevel.AtMostOnce,
                false);
        }

        /// <inheritdoc/>
        public ushort Publish(
            string topic,
            byte[] message)
        {
            return Publish(
                topic,
                message,
                null,
                null,
                MqttQoSLevel.AtMostOnce,
                false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Close();
        }
    }
}
