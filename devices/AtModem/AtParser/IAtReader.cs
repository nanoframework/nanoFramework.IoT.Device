﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;

namespace IoT.Device.AtModem
{
    /// <summary>
    /// Represents a reader for reading AT command responses.
    /// </summary>
    public interface IAtReader
    {
        /// <summary>
        /// Asynchronously reads a line of AT command response.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>A task representing the asynchronous read operation. The task result contains the read line.</returns>
        string ReadAsync(CancellationToken cancellationToken = default);

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