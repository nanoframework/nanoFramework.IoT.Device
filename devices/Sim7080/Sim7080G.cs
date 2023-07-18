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
        /// Determine wether connection sequence has been started, to wait for <see cref="ConnectionStatus"/> acknowledgement.
        /// </summary>
        private bool _sequenceStarted;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sim7080G"/> class.
        /// </summary>
        /// <param name="serialPort">The UART <see cref="SerialPort"/> for communication with the modem.</param>
        public Sim7080G(SerialPort serialPort)
        {
            _serialPort = serialPort;   
        }

        /// <summary>
        /// Reset Module when crashed.
        /// </summary>
        /// <param name="serialPort">The UART <see cref="SerialPort"/> for communication with the modem.</param>
        public static void Reset(SerialPort serialPort)
        {
            SimController.Reset(serialPort);
        }

        private NetworkInformation _networkInformation;

        /// <summary>
        /// Gets the current Network Information.
        /// </summary>
        public NetworkInformation NetworkInformation
        {
            get
            {
                if (_networkInformation == null)
                {
                    _networkInformation = new NetworkInformation();

                    SimController.GetNetworkInformation(_serialPort);
                }

                return _networkInformation;
            }

            private set
            {
                _networkInformation = value;
            }
        }

        private SimCardInformation _simCardInformation;

        /// <summary>
        /// Gets the current Sim Card Information.
        /// </summary>
        public SimCardInformation SimCardInformation
        {
            get
            {
                if (_simCardInformation == null)
                {
                    _simCardInformation = new SimCardInformation();

                    SimController.GetSimCardInformation(_serialPort);
                }

                return _simCardInformation;
            }

            private set
            {
                _simCardInformation = value;
            }
        }

        private DeviceInformation _deviceInformation;

        /// <summary>
        /// Gets the current Device Information.
        /// </summary>
        public DeviceInformation DeviceInformation
        {
            get
            {
                if (_deviceInformation == null)
                {
                    _deviceInformation = new DeviceInformation();

                    SimController.GetDeviceInformation(_serialPort);
                }

                return _deviceInformation;
            }

            private set
            {
                _deviceInformation = value;
            }
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

        #endregion

        /// <summary>
        /// Set Flow Control.
        /// </summary>
        /// <param name="softwareFlowMode">The desired <see cref="SoftwareFlowMode"/>.</param>
        public void SetFlowControl(SoftwareFlowMode softwareFlowMode)
        {
            SimController.SetFlowControl(_serialPort, softwareFlowMode);
        }

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

            _sequenceStarted = false;

            var retryCount = 0;

            do
            {
                if (!_sequenceStarted)
                {
                    retryCount++;

                    _sequenceStarted = SimController.NetworkConnect(_serialPort, apn, retryCount);
                }
            }
            while (NetworkInformation.ConnectionStatus != ConnectionStatus.Connected && retryCount < Retry);

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
        /// Connect to generic endpoint using MQTT protocol.
        /// </summary>
        /// <param name="clientId">The device/client identifier.</param>
        /// <param name="endpointUrl">The endpoint URL.</param>
        /// <param name="portNumber">The MQTT port number.</param>
        /// <param name="username">The user name for endpoint authentication.</param>
        /// <param name="password">The password for endpoint authentication.</param>
        /// <param name="wait">The time to wait to establish the connection.</param>
        /// <returns><see cref="ConnectionStatus"/></returns>
        public ConnectionStatus ConnectEndpoint(string clientId, string endpointUrl, int portNumber, string username, string password, int wait = 3000)
        {
            if (!_serialPort.IsOpen || NetworkInformation.ConnectionStatus == ConnectionStatus.Disconnected)
            {
                EndpointConnected = ConnectionStatus.Disconnected;
                return EndpointConnected;
            }

            _sequenceStarted = false;

            var retryCount = 0;

            do
            {
                if (!_sequenceStarted)
                {
                    retryCount++;

                    _sequenceStarted = SimController.EndpointConnect(_serialPort, clientId, endpointUrl, portNumber, username, password, wait);
                }
            }
            while (EndpointConnected != ConnectionStatus.Connected && retryCount < Retry);

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
        /// Send a message to Device-2-Cloud endpoint.
        /// </summary>
        /// <param name="message">The data message.</param>
        /// <param name="publicationTopic">The Device-2-Cloud publication topic Name.</param>
        /// <returns>A boolean indicating if the message was sent successfully.</returns>
        public bool SendMessage(string message, string publicationTopic)
        {
            return SimController.SendMessage(_serialPort, message, publicationTopic);
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
        /// Read and interpret AT Acknowledgement response.
        /// </summary>
        /// <returns>The initial Acknowlegdment message.</returns>
        public string ReadResponse()
        {
            string readLine = null;

            if (!_serialPort.IsOpen)
            {
                NetworkInformation.ConnectionStatus = ConnectionStatus.Disconnected;
                return readLine;
            }

            try
            {
                readLine = _serialPort.ReadLine();

                // Detect and remember new acknowledgement message
                ExtractAcknowledgement(readLine);

                // Skip, only read acknowledgement information
                if ((readLine.StartsWith(_acknowledgement) && !readLine.Contains("=")) ||
                    readLine == "\r" ||
                    readLine.StartsWith("OK"))
                {                
                    return readLine;
                }

                if (_acknowledgement.StartsWith("AT+SMCONF"))
                { 
                    Notify("MQTT_Configuration", AcknowledgementTranslator.Clean(readLine));
                    return readLine;
                }

                // Derive information from acknowledgement
                switch (_acknowledgement)
                {
                    case "ERROR":
                        Notify("ERROR", AcknowledgementTranslator.Clean(readLine));
                        break;
                    case "AT+CGMI":
                        DeviceInformation.Manufacturer = AcknowledgementTranslator.Clean(readLine);
                        Notify("DeviceInformation_Manufacturer", DeviceInformation.Manufacturer);
                        break;
                    case "AT+CGMM":
                        DeviceInformation.Model = AcknowledgementTranslator.Clean(readLine);
                        Notify("DeviceInformation_Manufacturer", DeviceInformation.Model);
                        break;
                    case "AT+CGMR":
                        DeviceInformation.FirmwareVersion = AcknowledgementTranslator.Clean(readLine);
                        Notify("DeviceInformation_FirmwareVersion", DeviceInformation.FirmwareVersion);
                        break;
                    case "ATI":
                        DeviceInformation.ProductNumber = AcknowledgementTranslator.Clean(readLine);
                        Notify("DeviceInformation_ProductNumber", DeviceInformation.ProductNumber);
                        break;
                    case string m when m.Contains("AT+CNSMOD"):
                        DeviceInformation.SystemMode = AcknowledgementTranslator.DeviceSystemMode(readLine);
                        Notify("DeviceInformation_SystemMode", DeviceInformation.SystemMode.ToString());
                        break;
                    case "AT+CGSN":
                        SimCardInformation.IMEI = AcknowledgementTranslator.Convert(readLine);
                        Notify("SimCardInformation_IMEI", SimCardInformation.IMEI.ToString());
                        break;
                    case "AT+CIMI":
                        SimCardInformation.IMSI = AcknowledgementTranslator.Convert(readLine);
                        Notify("SimCardInformation_IMSI", SimCardInformation.IMSI.ToString());
                        break;
                    case "AT+CCID":
                        SimCardInformation.ICCM = AcknowledgementTranslator.Clean(readLine);
                        Notify("SimCardInformation_ICCM", SimCardInformation.ICCM.ToString());
                        break;
                    case "AT+COPS?":
                        NetworkInformation.NetworkOperator = AcknowledgementTranslator.Extract(readLine);
                        Notify("NetworkInformation_NetworkOperator", NetworkInformation.NetworkOperator.ToString());
                        break;
                    case "AT+CGPADDR":
                        NetworkInformation.IPAddress = AcknowledgementTranslator.NetworkIPAddress(readLine, NetworkInformation.IPAddress);
                        Notify("NetworkInformation_IPAddress", NetworkInformation.IPAddress.ToString());
                        break;
                    case "AT+CGATT?":
                        NetworkInformation.ConnectionStatus = AcknowledgementTranslator.NetworkConnectionStatus(readLine);
                        Notify("NetworkInformation_ConnectionStatus", NetworkInformation.ConnectionStatus.ToString());
                        _sequenceStarted = false;
                        break;
                    case "AT+CSQ":
                        NetworkInformation.SignalQuality = AcknowledgementTranslator.NetworkSignalQuality(readLine);
                        Notify("NetworkInformation_SignalQuality", NetworkInformation.SignalQuality.ToString());
                        break;
                    case "AT+SMCONN":
                        EndpointConnected = AcknowledgementTranslator.EndpointConnectionStatus(readLine);
                        Notify("MQTT_EndpointConnected", EndpointConnected.ToString());
                        _sequenceStarted = false;
                        break;
                }
            }
            catch (TimeoutException timeoutException)
            {
                Notify("TimeoutException", timeoutException.Message, true);
            }
            catch (Exception exception)
            {
                Notify("Exception", exception.Message, true);
            }

            return readLine;
        }

        /// <summary>
        /// Check message for AT command.
        /// </summary>
        /// <param name="line">A acknowledgement message line.</param>
        private void ExtractAcknowledgement(string line)
        {
            if (line.StartsWith("AT"))
            {
                _acknowledgement = line.Split('\r')[0];
            }
        }

        /// <summary>
        /// Write Console Notification.
        /// </summary>
        /// <param name="category">The message category.</param>
        /// <param name="message">The notification message.</param>
        /// <param name="isDebug">Only write to console in debug mode when true.</param>
        internal static void Notify(string category, string message, bool isDebug = false)
        {
            var notification = $"[{category.PadRight(35, '.')}] {message}";

            if (isDebug)
            {
                Debug.WriteLine(notification);
            }
            else
            {
                Console.WriteLine(notification);
            }
        }
    }
}