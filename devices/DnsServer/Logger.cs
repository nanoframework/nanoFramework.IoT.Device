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
        /// <param name="exception">Optional exception to include in the log.</param>
        /// <param name="message">Optional message to log.</param>
        /// <param name="args">Optional values for message formatting.</param>
        public static void Log(
            LogLevel logLevel,
            Exception exception = null,
            string message = null,
            params object[] args)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            string formattedMessage = $"[DNS Server] {message}";

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

            GlobalLogger.Log(
                logLevel,
                exception,
                formattedMessage,
                args);
        }

        /// <summary>
        /// Log a debug level message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="args">Optional values for message formatting.</param>
        public static void Debug(string message, params object[] args) => Log(LogLevel.Debug, null, message, args);

        /// <summary>
        /// Log an information level message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="args">Optional values for message formatting.</param>
        public static void Information(string message, params object[] args) => Log(LogLevel.Information, null, message, args);

        /// <summary>
        /// Log a warning level message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="args">Optional values for message formatting.</param>
        public static void Warning(string message, params object[] args) => Log(LogLevel.Warning, null, message, args);

        /// <summary>
        /// Log a warning level message.
        /// </summary>
        /// <param name="exception">Optional exception to include in the log.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="args">Optional values for message formatting.</param>
        public static void Warning(Exception exception = null, string message = null, params object[] args) => Log(LogLevel.Warning, exception, message, args);

        /// <summary>
        /// Log an error level message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="args">Optional values for message formatting.</param>
        public static void Error(string message, params object[] args) => Log(LogLevel.Error, null, message, args);

        /// <summary>
        /// Log an error level message.
        /// </summary>
        /// <param name="exception">Optional exception to include in the log.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="args">Optional values for message formatting.</param>
        public static void Error(Exception exception, string message = null, params object[] args) => Log(LogLevel.Error, exception, message, args);
    }
}