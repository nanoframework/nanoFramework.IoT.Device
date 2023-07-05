// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

namespace IoT.Device.Sim7080
{
    internal static class SimController
    {
        /// <summary>
        /// Set Flow Control.
        /// </summary>
        /// <param name="serialPort">The UART <see cref="SerialPort"/> for communication with the modem.</param>
        /// <param name="softwareFlowMode">The desired <see cref="SoftwareFlowMode"/>.</param>
        public static void SetFlowControl(SerialPort serialPort, SoftwareFlowMode softwareFlowMode)
        {
            ExecuteCommand(serialPort, "AT+ICF?");
            ExecuteCommand(serialPort, $"AT+IFC={(int)softwareFlowMode},{(int)softwareFlowMode}");
        }

        /// <summary>
        /// Get the device information.
        /// </summary>
        /// <param name="serialPort">The UART <see cref="SerialPort"/> for communication with the modem.</param>
        public static void GetDeviceInformation(SerialPort serialPort)
        {
            try
            {
                // Request manufacturer identification
                ExecuteCommand(serialPort, "AT+CGMI");

                // Request model identification 
                ExecuteCommand(serialPort, "AT+CGMM");

                // Request firmware version identification  
                ExecuteCommand(serialPort, "AT+CGMR");

                // Request product type number
                ExecuteCommand(serialPort, "ATI");
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        /// <summary>
        /// Get SIM card information details.
        /// </summary>
        /// <param name="serialPort">The UART <see cref="SerialPort"/> for communication with the modem.</param>
        public static void GetSimCardInformation(SerialPort serialPort)
        {
            try
            {
                // Request IMEI
                ExecuteCommand(serialPort, "AT+CGSN");

                // Request IMSI of the SIM card
                ExecuteCommand(serialPort, "AT+CIMI");

                // Request CCID of the SIM card
                ExecuteCommand(serialPort, "AT+CCID");
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        /// <summary>
        /// Get network provider network information.
        /// </summary>
        /// <param name="serialPort">The UART <see cref="SerialPort"/> for communication with the modem.</param>
        public static void GetNetworkInformation(SerialPort serialPort)
        {
            try
            {
                // Return current Operator
                ExecuteCommand(serialPort, "AT+COPS?");

                // Read Signal Quality
                ExecuteCommand(serialPort, "AT+CSQ");
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        /// <summary>
        /// Get available network system modes for wireless and cellular network communication service.
        /// </summary>
        /// <param name="serialPort">The UART <see cref="SerialPort"/> for communication with the modem.</param>
        public static void GetSystemMode(SerialPort serialPort)
        {
            try
            {
                ExecuteCommand(serialPort, "AT+CBANDCFG=?");
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        /// <summary>
        /// Set the network system mode for wireless and cellular network communication service.
        /// </summary>
        /// <param name="serialPort">The UART <see cref="SerialPort"/> for communication with the modem.</param>
        /// <param name="systemMode">The desired <see cref="SystemMode"/>.</param>
        /// <param name="enableReporting">Report the network system mode information.</param>
        public static void SetSystemMode(SerialPort serialPort, SystemMode systemMode, bool enableReporting = true)
        {
            try
            {
                var reportingEnabled = enableReporting ? 1 : 0;

                ExecuteCommand(serialPort, $"AT+CNSMOD={reportingEnabled},{(int)systemMode}");
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        /// <summary>
        /// Connect to the cellular network with PDP context.
        /// It offers a packet data connection over which the device and the mobile network can exchange IP packets.
        /// </summary>
        /// <param name="serialPort">The UART <see cref="SerialPort"/> for communication with the modem.</param>
        /// <param name="apn">Cellular network access point name.</param>
        /// <param name="retryCount">The number of retries after error.</param>
        public static void NetworkConnect(SerialPort serialPort, string apn, int retryCount)
        {
            try
            {
                if (retryCount > 1)
                {
                    // Deactive App Network on error
                    ExecuteCommand(serialPort, "AT+CNACT=0,0");
                }

                // Define PDP Context, configure APN
                ExecuteCommand(serialPort, $"AT+CGDCONT=1,\"IP\",\"{apn}\"");

                // Query the configured APN
                ExecuteCommand(serialPort, "AT+CGDCONT?");

                // Activate PDP context, assign IP
                ExecuteCommand(serialPort, "AT+CNACT=0,1");

                // Validate if device received an IP address
                ExecuteCommand(serialPort, "AT+CGPADDR");

                // Check network connection
                ExecuteCommand(serialPort, "AT+CGATT?");
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);

                Reset(serialPort);
            }
        }

        /// <summary>
        /// Disconnect to the cellular network, deactivate Packet Data Protocol (PDP) context.
        /// </summary>
        /// <param name="serialPort">The UART <see cref="SerialPort"/> for communication with the modem.</param>
        /// <returns><see cref="ConnectionStatus"/></returns>
        public static ConnectionStatus NetworkDisconnect(SerialPort serialPort)
        {
            try
            {
                ExecuteCommand(serialPort, "AT+CNACT=0,0");

                return ConnectionStatus.Disconnected;
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);

                Reset(serialPort);

                return ConnectionStatus.Error;
            }
        }

        /// <summary>
        /// Connect to MQTT Endpoint.
        /// </summary>
        /// <param name="serialPort">The UART <see cref="SerialPort"/> for communication with the modem.</param>
        /// <param name="clientId">The device/client identifier.</param>
        /// <param name="endpointUrl">The endpoint url.</param>
        /// <param name="portNumber">The endpoint port number.</param>
        /// <param name="username">The username for authentication.</param>
        /// <param name="password">The password for authentication.</param>
        /// <param name="retryCount">The number of retries after error.</param>
        /// <param name="wait">The time to wait to establish the connection.</param>
        public static void EndpointConnect(SerialPort serialPort, string clientId, string endpointUrl, int portNumber, string username, string password, int retryCount, int wait)
        {
            try
            {
                // Time to wait should be minimal the same as serial port write timout
                wait = serialPort.WriteTimeout > wait ? serialPort.WriteTimeout : wait;

                // Simcom module MQTT parameter that sets the server URL and port
                ExecuteCommand(serialPort, $"AT+SMCONF=\"URL\",\"{endpointUrl}\",\"{portNumber}\"");

                // Set MQTT time to connect server
                ExecuteCommand(serialPort, "AT+SMCONF=\"KEEPTIME\",60");

                // Delete messages after they have been successfully sent
                ExecuteCommand(serialPort, "AT+SMCONF=\"CLEANSS\",1");

                // Send packet QOS level, at least once.
                ExecuteCommand(serialPort, "AT+SMCONF=\"QOS\",2");

                // Simcom module MQTT parameter that sets the client id
                ExecuteCommand(serialPort, $"AT+SMCONF=\"CLIENTID\",\"{clientId}\"");

                // Simcom module MQTT parameter that sets the user name
                ExecuteCommand(serialPort, $"AT+SMCONF=\"USERNAME\",\"{username}\"");

                // Simcom module MQTT parameter that sets the secure access token
                ExecuteCommand(serialPort, $"AT+SMCONF=\"PASSWORD\",\"{password}\"");

                // Simcom module MQTT open the connection
                ExecuteCommand(serialPort, "AT+SMCONN", wait);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);

                Reset(serialPort);
            }
        }

        /// <summary>
        /// Subscribe to Cloud-2-Device Topic.
        /// </summary>
        /// <param name="serialPort">The UART <see cref="SerialPort"/> for communication with the modem.</param>
        /// <param name="topic">The name of the topic.</param>
        /// <returns><see cref="ConnectionStatus"/></returns>
        public static ConnectionStatus SubscribeToTopic(SerialPort serialPort, string topic)
        {
            try
            {
                ExecuteCommand(serialPort, $"AT+SMSUB=\"{topic}\",1");

                return ConnectionStatus.Connected;
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);

                return ConnectionStatus.Error;
            }
        }

        /// <summary>
        /// Unsubscribe to Cloud-2-Device Topic. 
        /// </summary>
        /// <param name="serialPort">The UART <see cref="SerialPort"/> for communication with the modem.</param>
        /// <param name="topic">The name of the topic.</param>
        /// <returns><see cref="ConnectionStatus"/></returns>
        public static ConnectionStatus UnsubscribeFromTopic(SerialPort serialPort, string topic)
        {
            try
            {
                ExecuteCommand(serialPort, $"AT+SMUNSUB=\"{topic}\"");

                return ConnectionStatus.Connected;
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);

                return ConnectionStatus.Error;
            }
        }

        /// <summary>
        /// Disconnect from MQTT Endpoint.
        /// </summary>
        /// <param name="serialPort">The UART <see cref="SerialPort"/> for communication with the modem.</param>
        /// <returns><see cref="ConnectionStatus"/></returns>
        public static ConnectionStatus EndpointDisconnect(SerialPort serialPort)
        {
            try
            {
                ExecuteCommand(serialPort, "AT+SMDISC");

                return ConnectionStatus.Disconnected;
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);

                Reset(serialPort);

                return ConnectionStatus.Error;
            }
        }

        /// <summary>
        /// Send message to the serial port.
        /// </summary>
        /// <param name="serialPort">The UART <see cref="SerialPort"/> for communication with the modem.</param>
        /// <param name="message">The telemertry message to be send.</param>
        /// <param name="topic">The topic the message is send to.</param>
        /// <returns>Returns the success status.</returns>
        public static bool SendMessage(SerialPort serialPort, string message, string topic)
        {
            try
            {
                // Simcom module MQTT subscribe to D2C topic
                ExecuteCommand(serialPort, $"AT+SMPUB=\"{topic}\",{message.Length},1,1");

                // Simcom module MQTT sends the message
                ExecuteCommand(serialPort, message);

                return true;
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);

                return false;
            }
        }

        /// <summary>
        /// Execute AT command.
        /// </summary>
        /// <param name="serialPort">The UART <see cref="SerialPort"/> for communication with the modem.</param>
        /// <param name="command">The AT command.</param>
        /// <param name="wait">The time to wait for the AT command to complete.</param>
        internal static void ExecuteCommand(SerialPort serialPort, string command, int wait = 2000)
        {
            serialPort.WriteLine($"{command}\r");

            Thread.Sleep(wait);
        }

        /// <summary>
        /// Reset Module.
        /// </summary>
        /// <param name="serialPort">The UART <see cref="SerialPort"/> for communication with the modem.</param>
        internal static void Reset(SerialPort serialPort)
        {
            ExecuteCommand(serialPort, "AT+CEDUMP=1");
        }
    }
}
