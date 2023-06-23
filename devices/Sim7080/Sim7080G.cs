// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace IoT.Device.Sim7080
{
    /// <summary>
    /// Sim7080G Multi-Band CAT-M(eMTC) and NB-IoT module.
    /// </summary>
    public class Sim7080G
    {
        /// <summary>
        /// The UART <see cref="SerialPort"/> for communication with the modem.
        /// </summary>
        private readonly SerialPort _serialPort;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sim7080G"/> class.
        /// </summary>
        /// <param name="serialPort">The UART <see cref="SerialPort"/> for communication with the modem.</param>
        public Sim7080G(SerialPort serialPort) => _serialPort = serialPort;

        /// <summary>
        /// Gets or sets number of retries on failure.
        /// </summary>
        public int Retry { get; set; } = 3;

        /// <summary>
        /// Gets the network system mode for wireless and cellular network communication service.
        /// </summary>
        public SystemMode SystemMode { get; private set; }

        /// <summary>
        /// Gets thecellular network connection status.
        /// </summary>
        public ConnectionStatus NetworkConnected { get; private set; } = ConnectionStatus.Disconnected;

        /// <summary>
        /// Gets MQTT endpoint connection status.
        /// </summary>
        public ConnectionStatus EndpointConnected { get; private set; } = ConnectionStatus.Disconnected;

        /// <summary>
        /// Gets MQTT subscription topic connection status.
        /// </summary>
        public ConnectionStatus TopicConnected { get; private set; } = ConnectionStatus.Disconnected;

        /// <summary>
        /// Gets the Cellular Network Operator.
        /// </summary>
        public string Operator { get; private set; } = "Unknown";

        /// <summary>
        /// Gets the Public IP Address.
        /// </summary>
        public string IPAddress { get; private set; } = "0.0.0.0";

        /// <summary>
        /// Gets or sets Cloud-2-Device subscription topic Name.
        /// </summary>
        public string SubTopic { get; set; }

        /// <summary>
        /// Gets or sets Device-2-Cloud publication topic Name.
        /// </summary>
        public string PubTopic { get; set; }

        /// <summary>
        /// Set the network system mode for wireless and cellular network communication service.
        /// </summary>
        /// <param name="systemMode">The desired <see cref="SystemMode"/>.</param>
        /// <param name="enableReporting">Report the network system mode information.</param>
        /// <param name="wait">The time to wait to switch system mode.</param>
        public void SetNetworkSystemMode(SystemMode systemMode = SystemMode.GSM, bool enableReporting = true, int wait = 5000)
        {
            SimController.SetSystemMode(_serialPort, systemMode, enableReporting);

            Thread.Sleep(wait);
        }

        /// <summary>
        /// Connect to the cellular network.
        /// </summary>
        /// <param name="apn">Cellular network access point name.</param>
        /// <returns><see cref="ConnectionStatus"/></returns>
        public ConnectionStatus NetworkConnect(string apn)
        {
            if (!_serialPort.IsOpen)
            {
                NetworkConnected = ConnectionStatus.Disconnected;
                return NetworkConnected;
            }

            var retryCount = 0;

            do
            {
                retryCount++;

                SimController.NetworkConnect(_serialPort, apn);
            }
            while (NetworkConnected == ConnectionStatus.Disconnected && retryCount < Retry);
            return NetworkConnected;
        }

        /// <summary>
        /// Disconnect to the cellular network .
        /// </summary>
        /// <returns><see cref="ConnectionStatus"/></returns>
        public ConnectionStatus NetworkDisconnect()
        {
            if (_serialPort.IsOpen &&
                NetworkConnected == ConnectionStatus.Connected)
            {
                SimController.NetworkDisconnect(_serialPort);
            }

            return NetworkConnected;
        }

        /// <summary>
        /// Connect to Azure IoT Hub using MQTT protocol.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="hubName">The Azure IoT Hub Name.</param>
        /// <param name="sasToken">The Secure Access Token.</param>
        /// <param name="portNumber">The MQTT port number.</param>
        /// <param name="apiVersion">The Azure IoT Hub API version.</param>
        /// <param name="wait">The time to wait to establish the connection.</param>
        /// <returns><see cref="ConnectionStatus"/></returns>
        public ConnectionStatus ConnectAzureIoTHub(string deviceId, string hubName, string sasToken, int portNumber = 8883, string apiVersion = "2021-04-12", int wait = 5000)
        {
            if (!_serialPort.IsOpen || NetworkConnected == ConnectionStatus.Disconnected)
            {
                EndpointConnected = ConnectionStatus.Disconnected;
                return EndpointConnected;
            }

            string username = $"{hubName}.azure-devices.net/{deviceId}/?api-version={apiVersion}";
            string endpointUrl = $"{hubName}.azure-devices.net";

            var retryCount = 0;

            do
            {
                retryCount++;

                EndpointConnected = SimController.EndpointConnect(_serialPort, deviceId, endpointUrl, portNumber, username, sasToken, wait);
            }
            while (EndpointConnected == ConnectionStatus.Disconnected && retryCount < Retry);

            // <see cref="https://learn.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-messages-c2d"/>
            SubTopic = $"devices/{deviceId}/messages/devicebound/#";

            // <see cref="https://learn.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-messages-d2c"/>
            PubTopic = $"devices/{deviceId}/messages/events/";

            return EndpointConnected;
        }

        /// <summary>
        /// Connect to generic endpoint using MQTT protocol.
        /// </summary>
        /// <param name="clientId">The device/client identifier.</param>
        /// <param name="endpointUrl">The endpoint URL.</param>
        /// <param name="portNumber">The MQTT port number.</param>
        /// <param name="username">The user name for endpoint authentication.</param>
        /// <param name="password">The password for endpoint authentication.</param>
        /// <param name="wait">The time to wait to establish the connection.</param>
        /// <returns><see cref="ConnectionStatus"/></returns>
        public ConnectionStatus ConnectEndpoint(string clientId, string endpointUrl, int portNumber, string username, string password, int wait = 5000)
        {
            if (!_serialPort.IsOpen || NetworkConnected == ConnectionStatus.Disconnected)
            {
                EndpointConnected = ConnectionStatus.Disconnected;
                return EndpointConnected;
            }

            var retryCount = 0;

            do
            {
                retryCount++;

                EndpointConnected = SimController.EndpointConnect(_serialPort, clientId, endpointUrl, portNumber, username, password, wait);
            }
            while (EndpointConnected == ConnectionStatus.Disconnected && retryCount < Retry);

            return EndpointConnected;
        }

        /// <summary>
        /// Subscribe to Cloud-2-Device Event Hub Topic.
        /// </summary>
        /// <param name="topic">Event Hub Topic Name, uses <see cref="SubTopic"/>when null.</param> 
        /// <returns><see cref="ConnectionStatus"/></returns>
        public ConnectionStatus Subscribe2Topic(string topic)
        {
            SubTopic = (topic != null) ? topic : SubTopic;

            var retryCount = 0;

            do
            {
                retryCount++;

                TopicConnected = SimController.SubscribeToTopic(_serialPort, topic);
            }
            while (TopicConnected == ConnectionStatus.Disconnected && retryCount < Retry);

            return TopicConnected;
        }

        /// <summary>
        /// Send a message to endpoint <see cref="PubTopic"/>.
        /// </summary>
        /// <param name="message">The data message.</param>
        /// <returns>A boolean indicating if the message was sent successfully.</returns>
        public bool SendMessage(string message)
        {
            return SimController.SendMessage(_serialPort, message, PubTopic);
        }

        /// <summary>
        /// Disconnect to Azure IoT Hub.
        /// </summary>
        /// <returns><see cref="ConnectionStatus"/></returns>
        public ConnectionStatus DisonnectAzureIoTHub()
        {
            if (_serialPort.IsOpen &&
                NetworkConnected == ConnectionStatus.Connected &&
                EndpointConnected == ConnectionStatus.Connected)
            {
                if (SubTopic != null)
                {
                    TopicConnected = (TopicConnected == ConnectionStatus.Connected) ?
                        SimController.UnsubscribeFromTopic(_serialPort, SubTopic) :
                        TopicConnected;
                }

                EndpointConnected = SimController.EndpointDisconnect(_serialPort);
            }

            return EndpointConnected;
        }

        /// <summary>
        /// Disconnect from generic endpoint.
        /// </summary>
        /// <returns><see cref="ConnectionStatus"/></returns>
        public ConnectionStatus DisonnectEndpoint()
        {
            if (_serialPort.IsOpen &&
                NetworkConnected == ConnectionStatus.Connected &&
                EndpointConnected == ConnectionStatus.Connected)
            {
                EndpointConnected = SimController.EndpointDisconnect(_serialPort);
            }

            return EndpointConnected;
        }

        /// <summary>
        /// Read AT Acknowledgement response.
        /// </summary>
        public void ReadResponse()
        {
            if (!_serialPort.IsOpen)
            {
                NetworkConnected = ConnectionStatus.Disconnected;
                return;
            }

            if (_serialPort.BytesToRead > 0)
            {
                byte[] buffer = new byte[_serialPort.BytesToRead];

                var bytesRead = _serialPort.Read(buffer, 0, buffer.Length);

                try
                {
                    string responseMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    switch (responseMessage)
                    {
                        case string m when m.Contains("ERROR"):

                            Debug.WriteLine(responseMessage);

                            break;
                        case string m when m.Contains("+COPS:"):

                            Operator = SimController.ExtractATResponse(responseMessage);

                            break;
                        case string m when m.Contains("+CNACT:"):

                            IPAddress = (IPAddress == "0.0.0.0") ?
                                SimController.ExtractATResponse(responseMessage) :
                                IPAddress;

                            NetworkConnected = (IPAddress == "0.0.0.0") ?
                                ConnectionStatus.Disconnected :
                                ConnectionStatus.Connected;

                            break;
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception.Message);
                }
            }
        }
    }
}