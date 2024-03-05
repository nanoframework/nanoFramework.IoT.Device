// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;

namespace Iot.Device.AtModem
{
    /// <summary>
    /// Represents a writer for sending AT commands.
    /// </summary>
    public interface IAtWriter
    {
        /// <summary>
        /// Writes raw bytes to the underlying stream.
        /// </summary>
        /// <param name="content">The array of bytes to send.</param>
        void Write(byte[] content);

        /// <summary>
        /// Asynchronously writes a line of AT command.
        /// </summary>
        /// <param name="command">The AT command string to write.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        void WriteLineAsync(string command, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously writes an SMS Protocol Data Unit (PDU) and Ctrl+Z character.
        /// </summary>
        /// <param name="smsPdu">The SMS Protocol Data Unit (PDU) to write.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        void WriteSmsPduAndCtrlZAsync(string smsPdu, CancellationToken cancellationToken = default);

        /// <summary>
        /// Closes the AT writer.
        /// </summary>
        void Close();
    }
}
