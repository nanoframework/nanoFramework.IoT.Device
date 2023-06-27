// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
            int start = message.IndexOf("+CSQ: ");
            message = message.Substring(start + 6);
            message = Clean(message);
            message = $"{message.Split(',')[0]}.{message.Split(',')[1]}";

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
            var systemMode = message.Split(',')[1];
            int.TryParse(systemMode, out int systemModeNumber);

            return (SystemMode)systemModeNumber;
        }

        /// <summary>
        /// Clean message, remove line feed.
        /// </summary>
        /// <param name="message">The acknowledgement message.</param>
        /// <returns>String without <see cref="LineFeed"/></returns>
        internal static string Clean(string message)
        {
            int index = message.IndexOf(LineFeed);
            var substring = message.Substring(0, index);
            return substring;
        }

        /// <summary>
        /// Clean message and convert to long.
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