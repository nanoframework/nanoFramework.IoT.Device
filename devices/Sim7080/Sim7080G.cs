// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.IO.Ports;
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
        /// The current acknowledgement message that is translated with the <see cref="AcknowledgementTranslator"/>
        /// </summary>
        private string _acknowledgement;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sim7080G"/> class.
        /// </summary>
        /// <param name="serialPort">The UART <see cref="SerialPort"/> for communication with the modem.</param>
        public Sim7080G(SerialPort serialPort)
        {
            _serialPort = serialPort;
        }

        /// <summary>
        /// Gets the current Network Information.
        /// </summary>
        public NetworkInformation NetworkInformation { get; private set; } = new NetworkInformation();

        /// <summary>
        /// Gets the current Sim Card Information.
        /// </summary>
        public SimCardInformation SimCardInformation { get; private set; } = new SimCardInformation();

        /// <summary>
        /// Gets the current Device Information.
        /// </summary>
        public DeviceInformation DeviceInformation { get; private set; } = new DeviceInformation();

        /// <summary>
        /// Read information from module.
        /// </summary>
        public void Initialize()
        {
            GetNetworkSystemMode();

            SimController.GetDeviceInformation(_serialPort);
            SimController.GetNetworkInformation(_serialPort);
            SimController.GetSimCardInformation(_serialPort);
        }

        /// <summary>
        /// Gets or sets number of retries on failure.
        /// </summary>
        public int Retry { get; set; } = 3;

        #region MQTT

        /// <summary>
        /// Gets MQTT endpoint connection status.
        /// </summary>
        public ConnectionStatus EndpointConnected { get; private set; } = ConnectionStatus.Disconnected;

        /// <summary>
        /// Gets MQTT subscription topic connection status.
        /// </summary>
        public ConnectionStatus TopicConnected { get; private set; } = ConnectionStatus.Disconnected;

        /// <summary>
        /// Gets or sets Cloud-2-Device subscription topic Name.
        /// </summary>
        public string SubTopic { get; set; }

        /// <summary>
        /// Gets or sets Device-2-Cloud publication topic Name.
        /// </summary>
        public string PubTopic { get; set; }

        #endregion

        /// <summary>
        /// Get the available network system mode for wireless and cellular network communication service.
        /// </summary>
        public void GetNetworkSystemMode()
        {
            SimController.GetSystemMode(_serialPort);
        }

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
                NetworkInformation.ConnectionStatus = ConnectionStatus.Disconnected;
                return NetworkInformation.ConnectionStatus;
            }

            var retryCount = 0;

            do
            {
                retryCount++;

                SimController.NetworkConnect(_serialPort, apn);
            }
            while (NetworkInformation.ConnectionStatus == ConnectionStatus.Disconnected && retryCount < Retry);
            return NetworkInformation.ConnectionStatus;
        }

        /// <summary>
        /// Disconnect to the cellular network .
        /// </summary>
        /// <returns><see cref="ConnectionStatus"/></returns>
        public ConnectionStatus NetworkDisconnect()
        {
            if (_serialPort.IsOpen &&
                NetworkInformation.ConnectionStatus == ConnectionStatus.Connected)
            {
                SimController.NetworkDisconnect(_serialPort);
            }

            return NetworkInformation.ConnectionStatus;
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
            if (!_serialPort.IsOpen || NetworkInformation.ConnectionStatus == ConnectionStatus.Disconnected)
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
            if (!_serialPort.IsOpen || NetworkInformation.ConnectionStatus == ConnectionStatus.Disconnected)
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
                NetworkInformation.ConnectionStatus == ConnectionStatus.Connected &&
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
                NetworkInformation.ConnectionStatus == ConnectionStatus.Connected &&
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
                NetworkInformation.ConnectionStatus = ConnectionStatus.Disconnected;
                return;
            }

            string readLine;

            while (_serialPort.BytesToRead > 0)
            {
                try
                {
                    readLine = _serialPort.ReadLine();

                    // Detect and remember new acknowledgement message
                    _acknowledgement = readLine.StartsWith("AT")
                        ? ExtractAcknowledgement(readLine)
                        : _acknowledgement;

                    // Skip, only read acknowledgement information
                    if (readLine.StartsWith(_acknowledgement) ||
                        readLine == "\r" ||
                        readLine.StartsWith("OK"))
                    {
                        continue;
                    }

                    // Derive information from acknowledgement
                    switch (_acknowledgement)
                    {
                        case "AT+CGNAPN":
                            // Save APN
                            break;
                        case "AT+CGDCONT?":
                            // PDP Context
                            break;
                        case "AT+CGACT?":
                            // PDP Context Activate or Deactivate
                            break;
                        case "AT+CBANDCFG=?":
                            // List of supported SystemMode
                            break;
                        case "AT+SNPDPID=0":
                            // PDP Selected for ping
                            break;
                        case "AT+CGMI":
                            DeviceInformation.Manufacturer = AcknowledgementTranslator.Clean(readLine);
                            Debug.WriteLine(DeviceInformation.Manufacturer);
                            break;
                        case "AT+CGMM":
                            DeviceInformation.Model = AcknowledgementTranslator.Clean(readLine);
                            Debug.WriteLine(DeviceInformation.Model);
                            break;
                        case "AT+CGMR":
                            DeviceInformation.FirmwareVersion = AcknowledgementTranslator.Clean(readLine);
                            Debug.WriteLine(DeviceInformation.FirmwareVersion);
                            break;
                        case "ATI":
                            DeviceInformation.ProductNumber = AcknowledgementTranslator.Clean(readLine);
                            Debug.WriteLine(DeviceInformation.ProductNumber);
                            break;
                        case "AT+CNSMOD":
                            DeviceInformation.SystemMode = AcknowledgementTranslator.DeviceSystemMode(readLine);
                            Debug.WriteLine(DeviceInformation.SystemMode.ToString());
                            break;
                        case "AT+CGSN":
                            SimCardInformation.IMEI = AcknowledgementTranslator.Convert(readLine);
                            Debug.WriteLine(SimCardInformation.IMEI.ToString());
                            break;
                        case "AT+CIMI":
                            SimCardInformation.IMSI = AcknowledgementTranslator.Convert(readLine);
                            Debug.WriteLine(SimCardInformation.IMSI.ToString());
                            break;
                        case "AT+CCID":
                            SimCardInformation.ICCM = AcknowledgementTranslator.Clean(readLine);
                            Debug.WriteLine(SimCardInformation.ICCM);
                            break;
                        case "AT+COPS?":
                            NetworkInformation.NetworkOperator = AcknowledgementTranslator.Extract(readLine);
                            Debug.WriteLine(NetworkInformation.NetworkOperator);
                            break;
                        case "AT+CGPADDR":
                            NetworkInformation.IPAddress = AcknowledgementTranslator.NetworkIPAddress(readLine, NetworkInformation.IPAddress);
                            Debug.WriteLine(NetworkInformation.IPAddress.ToString());
                            break;
                        case "AT+CGATT?":
                            NetworkInformation.ConnectionStatus = AcknowledgementTranslator.NetworkConnectionStatus(readLine);
                            Debug.WriteLine(NetworkInformation.ConnectionStatus.ToString());
                            break;
                        case "AT+CSQ":
                            NetworkInformation.SignalQuality = AcknowledgementTranslator.NetworkSignalQuality(readLine);
                            Debug.WriteLine(NetworkInformation.SignalQuality.ToString());
                            break;
                        default:
                            Debug.WriteLine($"{_acknowledgement}[IGNORED]");
                            break;
                    }
                }
                catch (TimeoutException timeoutException)
                {
                    Debug.WriteLine(timeoutException.Message);
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception.Message);
                }
            }
        }

        private string ExtractAcknowledgement(string line)
        {
            return line.Split('\r')[0];
        }
    }
}