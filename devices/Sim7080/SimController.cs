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
        /// <returns><see cref="ConnectionStatus"/></returns>
        public static ConnectionStatus NetworkConnect(SerialPort serialPort, string apn)
        {
            try
            {
                // Get Network APN in CAT-M or NB-IoT
                ExecuteCommand(serialPort, "AT+CGNAPN");

                // Define PDP Context, configure APN
                ExecuteCommand(serialPort, $"AT+CGDCONT=1,\"IP\",\"{apn}\"");

                // Query the configured APN
                ExecuteCommand(serialPort, "AT+CGDCONT?");

                // Activate the PDP (packet data protocol) context
                ExecuteCommand(serialPort, "AT+CGACT=1,1");
                ExecuteCommand(serialPort, "AT+CGACT?");

                // Validate if device received an IP address
                ExecuteCommand(serialPort, "AT+CGPADDR");

                // Set the APN
                ExecuteCommand(serialPort, $"AT+CNCFG=0,1,\"{apn}\"");

                // Select PDP
                ExecuteCommand(serialPort, "AT+SNPDPID=0");

                // Activate PDP context, assign IP
                ExecuteCommand(serialPort, "AT+CNACT=0,1");

                // Check network connection
                ExecuteCommand(serialPort, "AT+CGATT?");

                return ConnectionStatus.Connected;
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);

                return ConnectionStatus.Error;
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
        /// <param name="wait">The time to wait to establish the connection.</param>
        /// <returns><see cref="ConnectionStatus"/></returns>
        public static ConnectionStatus EndpointConnect(SerialPort serialPort, string clientId, string endpointUrl, int portNumber, string username, string password, int wait = 5000)
        {
            try
            {
                // Quality of Service 
                ExecuteCommand(serialPort, "AT+SMCONF=\"QOS\",1");

                // Simcom module MQTT parameter that sets the server URL and port
                ExecuteCommand(serialPort, $"AT+SMCONF=\"URL\",\"{endpointUrl}\",\"{portNumber}\"");

                // Set MQTT time to connect server
                ExecuteCommand(serialPort, "AT+SMCONF=\"KEEPTIME\",60");

                // Delete messages after they have been successfully sent
                ExecuteCommand(serialPort, "AT+SMCONF=\"CLEANSS\",1");

                // Simcom module MQTT parameter that sets the client id
                ExecuteCommand(serialPort, $"AT+SMCONF=\"CLIENTID\",\"{clientId}\"");

                // Simcom module MQTT parameter that sets the api endpoint for the specific device
                ExecuteCommand(serialPort, $"AT+SMCONF=\"USERNAME\",\"{username}\"");

                // Simcom module MQTT parameter that sets the secure access token
                ExecuteCommand(serialPort, $"AT+SMCONF=\"PASSWORD\",\"{password}\"");

                // Simcom module MQTT open the connection
                ExecuteCommand(serialPort, "AT+SMCONN", wait);

                return ConnectionStatus.Connected;
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);

                ExecuteCommand(serialPort, "+CEDUMP=1");

                return ConnectionStatus.Error;
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
        internal static void ExecuteCommand(SerialPort serialPort, string command, int wait = 1000)
        {
            serialPort.WriteLine($"{command}\r");

            Thread.Sleep(wait);
        }
    }
}
