// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Logging;
using System;

namespace Iot.Device.DnsServer
{
    /// <summary>
    /// Static logging utility class for DNS server components.
    /// </summary>
    internal static class Logger
    {
        /// <summary>
        /// Gets or sets the global logger instance.
        /// </summary>
        public static ILogger GlobalLogger { get; set; }

        /// <summary>
        /// Log a message with the specified log level.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">Optional exception to include in the log.</param>
        /// <param name="component">Optional component name for log source identification.</param>
        public static void Log(LogLevel logLevel, string message, Exception exception = null, string component = "DNS Server")
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            string formattedMessage = $"[{component}] {message}";

            if (GlobalLogger is null)
            {
#if !DEBUG
                if (logLevel > LogLevel.Debug)
                {
#endif
                Console.WriteLine(formattedMessage);
#if !DEBUG
                }
#endif
                return;
            }

            GlobalLogger.Log(logLevel, exception, formattedMessage);
        }

        /// <summary>
        /// Log a debug level message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="component">Optional component name for log source identification.</param>
        public static void Debug(string message, string component = "DNS Server")
        {
            Log(LogLevel.Debug, message, null, component);
        }

        /// <summary>
        /// Log an information level message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="component">Optional component name for log source identification.</param>
        public static void Information(string message, string component = "DNS Server")
        {
            Log(LogLevel.Information, message, null, component);
        }

        /// <summary>
        /// Log a warning level message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">Optional exception to include in the log.</param>
        /// <param name="component">Optional component name for log source identification.</param>
        public static void Warning(string message, Exception exception = null, string component = "DNS Server")
        {
            Log(LogLevel.Warning, message, exception, component);
        }

        /// <summary>
        /// Log an error level message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">Optional exception to include in the log.</param>
        /// <param name="component">Optional component name for log source identification.</param>
        public static void Error(string message, Exception exception = null, string component = "DNS Server")
        {
            Log(LogLevel.Error, message, exception, component);
        }
    }
}