// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using IoT.Device.AtModem.CodingSchemes;
using IoT.Device.AtModem.Events;
using IoT.Device.AtModem.FileStorage;
using IoT.Device.AtModem.Modem;
using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;
using System;
using System.Collections;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace IoT.Device.AtModem.Mqtt
{
    /// <summary>
    /// MQTT Client for SIM7080.
    /// </summary>
    public class Sim7080MqttClient : IMqttClient
    {
        private const string CaCertName = "ca.crt";
        private const string ClientCertName = "cl.crt";
        private const int IndexSSL = 1;
        private ModemBase _modem;
        private string _brokerHostName;
        private int _brokerPort;
        private bool _secure;
        private ushort _messageId = 0;
        private string _caCertName;
        private string _clCertName;
        private ManualResetEvent _promptArived = new ManualResetEvent(false);

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

                // Split after the first comma in the line
                var elements = line.Split(new char[] { ',' }, 2);
                var topic = elements[0].Trim('"');
                var message = elements[1].Trim('"');

                // There is no trace of QoS Level or retain flag in the response
                MqttMsgPublishReceived?.Invoke(sender, new MqttMsgPublishEventArgs(topic, Encoding.UTF8.GetBytes(message), false, MqttQoSLevel.AtMostOnce, false));
            }
            else if (e.Message == ">")
            {
                _promptArived.Set();
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
                var preivousStorage = ((Sim7080FileStorage)_modem.FileStorage).Storage;
                ((Sim7080FileStorage)_modem.FileStorage).Storage = Sim7080FileStorage.StorageDirectory.Customer;
                if (_modem.FileStorage.GetFileSize(_caCertName) > 0)
                {
                    _modem.FileStorage.DeleteFile(_caCertName);
                }

                _modem.FileStorage.WriteFile(_caCertName, caCert);
                ((Sim7080FileStorage)_modem.FileStorage).Storage = preivousStorage;
            }

            if (clientCert != null)
            {
                int hash = HashHelper.ComputeHash(clientCert);

                // Check first if the file exists already
                _caCertName = hash + CaCertName;
                var preivousStorage = ((Sim7080FileStorage)_modem.FileStorage).Storage;
                ((Sim7080FileStorage)_modem.FileStorage).Storage = Sim7080FileStorage.StorageDirectory.Customer;
                _clCertName = hash + ClientCertName;
                if (_modem.FileStorage.GetFileSize(_clCertName) > 0)
                {
                    _modem.FileStorage.DeleteFile(_clCertName);
                }

                _modem.FileStorage.WriteFile(_clCertName, clientCert);
                ((Sim7080FileStorage)_modem.FileStorage).Storage = preivousStorage;
            }
        }

        /// <inheritdoc/>
        public MqttReasonCode Connect(string clientId, string username, string password, bool willRetain, MqttQoSLevel willQosLevel, bool willFlag, string willTopic, string willMessage, bool cleanSession, ushort keepAlivePeriod)
        {
            int retries = 3;

            // Server URL and port
            var response = _modem.Channel.SendCommand($"AT+SMCONF=\"URL\",\"{_brokerHostName}\",{_brokerPort}");
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
            response = _modem.Channel.SendCommand($"AT+SMCONF=\"TOPIC\",\"{willTopic}\"");
            if (!response.Success)
            {
                return MqttReasonCode.UnspecifiedError;
            }

            // Enable SSL
            // 0 QAPI_NET_SSL_PROTOCOL_UNKNOWN
            // 1 QAPI_NET_SSL_PROTOCOL_TLS_1_0
            // 2 QAPI_NET_SSL_PROTOCOL_TLS_1_1
            // 3 QAPI_NET_SSL_PROTOCOL_TLS_1_2
            // 4 QAPI_NET_SSL_PROTOCOL_DTLS_1_0
            // 5 QAPI_NET_SSL_PROTOCOL_DTLS_1_2
            int sslVersion = 0;
            switch (_sslProtocol)
            {
                default:
                    sslVersion = 0;
                    break;
                case MqttSslProtocols.TLSv1_0:
                    sslVersion = 1;
                    break;
                case MqttSslProtocols.TLSv1_1:
                    sslVersion = 2;
                    break;
                case MqttSslProtocols.TLSv1_2:
                    sslVersion = 3;
                    break;
            }

            _modem.Channel.SendCommand($"AT+CSSLCFG=\"SSLVERSION\",{IndexSSL},{sslVersion}");

            // 1 = TLS
            _modem.Channel.SendCommand($"AT+CSSLCFG=\"PROTOCOL\",{IndexSSL},1");

            _modem.Channel.SendCommand($"AT+CSSLCFG=\"IGNORERTCTIME\",{IndexSSL},1");

            if (_caCertName != null)
            {
                _modem.Channel.SendCommand($"AT+CSSLCFG=\"CONVERT\",2,\"{_caCertName}\"");
            }

            if (_clCertName != null)
            {
                _modem.Channel.SendCommand($"AT+CSSLCFG=\"CONVERT\",1,\"{_clCertName}\"");
            }

            // Set the SSL parameters, this is using the index 1, that may have to be updated somewhow
            _modem.Channel.SendCommand($"AT+SMSSL={IndexSSL},\"{(_caCertName != null ? _caCertName : string.Empty)}\",\"{(_caCertName != null ? _clCertName : string.Empty)}\"");

        RetryConnect:
            // Simcom module MQTT open the connection
            response = _modem.Channel.SendCommand("AT+SMCONN", TimeSpan.FromMinutes(1));
            if (!response.Success)
            {
                if (IsStillConnected())
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

            IsConnected = true;
            return MqttReasonCode.Success;
        }

        /// <inheritdoc/>
        public void Disconnect()
        {
            // Disconnect from the MQTT server
            _modem.Channel.SendCommand("AT+SMDISC");
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
            _promptArived.Reset();
            _modem.Channel.SendBytesWithoutAck(Encoding.UTF8.GetBytes($"AT+SMPUB=\"{topic}\",{message.Length},{(int)qosLevel},{(retain ? "1" : "0")}\r\n"));
            Debug.WriteLine($"Out: AT+SMPUB=\"{topic}\",{message.Length},{(int)qosLevel},{(retain ? "1" : "0")}");

            CancellationTokenSource cts = new CancellationTokenSource(1000);
            cts.Token.WaitHandle.WaitOne(1000, true);

            if (message.Length > 0)
            {
                _modem.Channel.SendBytesWithoutAck(message);
            }
            else
            {
                // This is needed to send an empty message
                Thread.Sleep(20);
            }

            // We don't check the status
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
            int retry = 3;
            int state = 0;
        RetryState:
            var response = _modem.Channel.SendCommandReadSingleLine("AT+SMSTATE?", "+SMSTATE");
            if (response.Success)
            {
                string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
                if (line.StartsWith("+SMSTATE: "))
                {
                    state = int.Parse(line.Substring(10));
                }
            }
            else
            {
                if (retry-- > 0)
                {
                    Thread.Sleep(1000);
                    goto RetryState;
                }
            }

            if (state == 0)
            {
                if (IsConnected)
                {
                    IsConnected = false;
                    ConnectionClosed?.Invoke(this, new EventArgs());
                }
            }

            return state != 0;
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
    }
}
