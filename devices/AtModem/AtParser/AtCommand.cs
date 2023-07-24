// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace IoT.Device.AtModem
{
    /// <summary>
    /// Represents an AT command.
    /// </summary>
    public class AtCommand
    {
        /// <summary>
        /// Initializes a new instance of the AtCommand class.
        /// </summary>
        /// <param name="commandType">The type of the AT command.</param>
        /// <param name="command">The AT command string.</param>
        /// <param name="responsePrefix">The expected response prefix.</param>
        /// <param name="smsPdu">The SMS Protocol Data Unit (PDU).</param>
        /// <param name="timeout">The timeout duration for the command.</param>
        public AtCommand(AtCommandType commandType, string command, string responsePrefix, string smsPdu, TimeSpan timeout)
        {
            CommandType = commandType;
            Command = command;
            ResponsePrefix = responsePrefix;
            SmsPdu = smsPdu;
            Timeout = timeout;
        }

        /// <summary>
        /// Gets the type of the AT command.
        /// </summary>
        public AtCommandType CommandType { get; }

        /// <summary>
        /// Gets the AT command string.
        /// </summary>
        public string Command { get; }

        /// <summary>
        /// Gets the expected response prefix.
        /// </summary>
        public string ResponsePrefix { get; }

        /// <summary>
        /// Gets or sets the SMS Protocol Data Unit (PDU).
        /// </summary>
        public string SmsPdu { get; set; }

        /// <summary>
        /// Gets the timeout duration for the command.
        /// </summary>
        public TimeSpan Timeout { get; }
    }
}
