// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using IoT.Device.Sim7080;
using nanoFramework.Hardware.Esp32;
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

namespace Sim7080.Sample
{
    public static class Program
    {
        static SerialPort _serialPort;
        static Sim7080G _sim;

        // Provider variables
        static readonly string _apn = "<YOUR-APN>";

        //// Azure EventHub variables
        static readonly string _deviceId = "<YOUR-DEVICE-NAME>";
        static readonly string _hubName = "<YOUR-IOT-HUB-NAME>";
        static readonly string _sasToken = "<YOUR-SAS-TOKEN>";

        // <see cref="https://learn.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-messages-c2d"/>
        static string subTopic = $"devices/{_deviceId}/messages/devicebound/#";

        // <see cref="https://learn.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-messages-d2c"/>
        static string pubTopic = $"devices/{_deviceId}/messages/events/";

        private static ConnectionStatus _networkConnected { get; set; } = ConnectionStatus.Disconnected;

        private static ConnectionStatus _endpointConnected { get; set; } = ConnectionStatus.Disconnected;

        public static void Main()
        {
            // Open serial port
            OpenSerialPort();

            // Setup an event handler that will fire when a char is received in the serial device input stream
            _serialPort.DataReceived += SerialDevice_DataReceived;

            _sim = new Sim7080G(_serialPort, true);

            // Switch to prefered network mode
            _sim.SetNetworkSystemMode(SystemMode.LTE_NB, false);

            // Connect to network access point
            _networkConnected = _sim.NetworkConnect(_apn);

            // Connect to Endpoint
            if (_networkConnected == ConnectionStatus.Connected)
            {
                string username = $"{_hubName}.azure-devices.net/{_deviceId}/?api-version={_apiVersion}";
                string endpointUrl = $"{_hubName}.azure-devices.net";

                _endpointConnected = _sim.ConnectEndpoint(_deviceId, endpointUrl, _portNumber, username, _sasToken);
            }

            if (_endpointConnected == ConnectionStatus.Connected)
            {
                _sim.SendMessage($"test{Guid.NewGuid()}", pubTopic);
            }

            // Disconnect from Endpoint
            if (_endpointConnected != ConnectionStatus.Disconnected)
            {
                _endpointConnected = _sim.DisonnectEndpoint();
            }

            // Disconnect from network access point
            if (_networkConnected != ConnectionStatus.Disconnected)
            {
                _networkConnected = _sim.NetworkDisconnect();
            }

            CloseSerialPort();

            Thread.Sleep(Timeout.Infinite);
        }

        /// <summary>
        /// Event raised when message is received from the serial port.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void SerialDevice_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var message = _sim?.ReadResponse();

            // Debug.WriteLine(message);
        }

        #region SerialPort

        /// <summary>
        /// Configure and open the serial port for communication.
        /// </summary>
        /// <param name="port"></param>
        /// <param name="baudRate"></param>
        /// <param name="parity"></param>
        /// <param name="stopBits"></param>
        /// <param name="handshake"></param>
        /// <param name="dataBits"></param>
        /// <param name="readBufferSize"></param>
        /// <param name="readTimeout"></param>
        /// <param name="writeTimeout"></param>
        /// <param name="watchChar"></param>
        private static void OpenSerialPort(
            string port = "COM3",
            int baudRate = 115200,
            Parity parity = Parity.None,
            StopBits stopBits = StopBits.One,
            Handshake handshake = Handshake.None,
            int dataBits = 8,
            int readTimeout = Timeout.Infinite,
            int writeTimeout = Timeout.Infinite)
        {
            // Configure GPIOs 16 and 17 to be used in UART2 (that's refered as COM3)
            Configuration.SetPinFunction(16, DeviceFunction.COM3_RX);
            Configuration.SetPinFunction(17, DeviceFunction.COM3_TX);

            _serialPort = new(port)
            {
                //Set parameters
                BaudRate = baudRate,
                Parity = parity,
                StopBits = stopBits,
                Handshake = handshake,
                DataBits = dataBits,
                ReadTimeout = readTimeout,
                WriteTimeout = writeTimeout
            };

            try
            {
                // Open the serial port
                _serialPort.Open();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        /// <summary>
        /// Close the serial port
        /// </summary>
        private static void CloseSerialPort()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
                _serialPort.Dispose();
            }
        }

        #endregion
    }
}
