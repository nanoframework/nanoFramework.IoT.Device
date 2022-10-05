// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using UnitsNet;

namespace Iot.Device.RadioReceiver
{
    /// <summary>
    /// Base class for radio receiver.
    /// </summary>
    public abstract class RadioReceiverBase : IDisposable
    {
        /// <summary>
        /// Gets or sets radio receiver FM frequency.
        /// </summary>
        public abstract Frequency Frequency { get; set; }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the RadioReceiverBase and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
