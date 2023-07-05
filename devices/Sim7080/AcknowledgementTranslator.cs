// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Net;

namespace IoT.Device.Sim7080
{
    /// <summary>
    /// Translate Modem Acknowledgement (ACK) messages to library variables.
    /// </summary>
    internal static class AcknowledgementTranslator
    {
        private static readonly string LineFeed = "\r";

        /// <summary>
        /// Determine device <see cref="ConnectionStatus"/> from ACK.
        /// </summary>
        /// <param name="message">The acknowledgement message.</param>
        /// <returns><see cref="ConnectionStatus"/></returns>
        internal static ConnectionStatus NetworkConnectionStatus(string message)
        {
            return message.Contains("+CGATT: 1") ?
                                    ConnectionStatus.Connected :
                                    ConnectionStatus.Disconnected;
        }

        /// <summary>
        /// Determine MQTT endpoint <see cref="ConnectionStatus"/> from ACK.
        /// </summary>
        /// <param name="message">The acknowledgement message.</param>
        /// <returns><see cref="ConnectionStatus"/></returns>
        internal static ConnectionStatus EndpointConnectionStatus(string message)
        {
            try
            {
                Debug.WriteLine($"!!{message}");

                if (message.Contains("OK"))
                {
                    return ConnectionStatus.Connected;
                }
                else if (message.Contains("ERROR"))
                {
                    return ConnectionStatus.Error;
                }
            } 
            catch
            {
                return ConnectionStatus.Error;
            }

            return ConnectionStatus.Disconnected;
        }

        /// <summary>
        /// Extract public <see cref="IPAddress"/> from ACK.
        /// </summary>
        /// <param name="message">The acknowledgement message.</param>
        /// <param name="ipAddress">The current <see cref="IPAddress"/></param>
        /// <returns><see cref="IPAddress"/></returns>
        internal static IPAddress NetworkIPAddress(string message, IPAddress ipAddress)
        {
            message = Clean(message);

            var ipString = message.Split(',')[1].Split('/')[0];

            if (ipAddress == null)
            {
                ipAddress = IPAddress.Parse(ipString);
            }

            return ipAddress;
        }

        /// <summary>
        /// Extract the signal strength from ACK.
        /// </summary>
        /// <param name="message">The acknowledgement message.</param>
        /// <returns>The signal quality.</returns>
        internal static double NetworkSignalQuality(string message)
        {
            if (!message.StartsWith("+CSQ: "))
            {
                return 0;
            }

            message = Clean(message);
            message = message.Substring(6);
            double.TryParse(message, out double signal);

            return signal;
        }

        /// <summary>
        /// Extract the <see cref="SystemMode"/> firmware from ACK.
        /// </summary>
        /// <param name="message">The acknowledgement message.</param>
        /// <returns><see cref="SystemMode"/></returns>
        internal static SystemMode DeviceSystemMode(string message)
        {
            var systemMode = Clean(message);
            systemMode = systemMode.Split('=')[1];
            systemMode = systemMode.Split(',')[1];
            int.TryParse(systemMode, out int systemModeNumber);

            return Interpret(systemModeNumber);
        }

        /// <summary>
        /// Parse integer to <see cref="SystemMode"/>
        /// </summary>
        /// <param name="systemModeNumber">Integer corresponding to <see cref="SystemMode"/> value.</param>
        /// <returns><see cref="SystemMode"/></returns>
        internal static SystemMode Interpret(int systemModeNumber)
        {
            switch (systemModeNumber)
            {
                case 1:
                    return SystemMode.GSM;
                case 3:
                    return SystemMode.EGPRS;
                case 7:
                    return SystemMode.LTE_M1;
                case 9:
                    return SystemMode.LTE_NB;
                default:
                    return SystemMode.NoService;
            }
        }

        /// <summary>
        /// Remove line feed.
        /// </summary>
        /// <param name="message">The acknowledgement message.</param>
        /// <returns>String without <see cref="LineFeed"/></returns>
        internal static string Clean(string message)
        {
            int index = message.IndexOf(LineFeed);
            if (index == 0) 
            { 
                index = LineFeed.Length; 
            }

            var substring = message.Substring(0, index);
            return substring;
        }

        /// <summary>
        /// Convert to long.
        /// </summary>
        /// <param name="message">The acknowledgement message.</param>
        /// <returns>Long without <see cref="LineFeed"/></returns>
        internal static long Convert(string message)
        {
            message = Clean(message);
            long.TryParse(message, out long longValue);
            return longValue;
        }

        /// <summary>
        /// Extract string from message.
        /// </summary>
        /// <param name="message">The acknowledgement message.</param>
        /// <returns>Extracted string.</returns>
        internal static string Extract(string message)
        {
            int start = message.IndexOf("\"");
            message = message.Substring(start + 1);
            int end = message.IndexOf("\"");
            return message.Substring(0, end);
        }
    }
}