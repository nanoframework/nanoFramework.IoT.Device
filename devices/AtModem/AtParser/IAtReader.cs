// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;

namespace Iot.Device.AtModem
{
    /// <summary>
    /// Represents a reader for reading AT command responses.
    /// </summary>
    public interface IAtReader
    {
        /// <summary>
        /// Reads a line of AT command response.
        /// </summary>
        /// <param name="endOfLine">The end of line string. If null, the default end of line will be used.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>A task representing the asynchronous read operation. The task result contains the read line.</returns>
        string Read(string endOfLine = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Reads a raw number of bytes on the channel. It will return the desired number of bytes even if it has to read less.
        /// </summary>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The bytes read.</returns>
        byte[] ReadBytes(int count);

        /// <summary>
        /// Reads a single line from the serial port. Without processing it.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The line read.</returns>
        string ReadSingleLine(CancellationToken cancellationToken = default);

        /// <summary>
        /// Opens the AT reader.
        /// </summary>
        void Open();

        /// <summary>
        /// Closes the AT reader.
        /// </summary>
        void Close();

        /// <summary>
        /// Gets the current number of items available in the reader.
        /// </summary>
        /// <returns>The number of available items.</returns>
        int AvailableItems();
    }
}
