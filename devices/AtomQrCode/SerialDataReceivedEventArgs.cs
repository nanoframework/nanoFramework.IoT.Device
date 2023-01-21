// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.AtomQrCode
{
    /// <summary>
    /// Provides data for the <see cref="QrCodeReader.BarcodeDataAvailable"/> event.
    /// </summary>
    public class BarcodeDataAvailableEventArgs : EventArgs
    {
        private readonly string _data;

        internal BarcodeDataAvailableEventArgs(string data)
        {
            _data = data;
        }

        /// <summary>
        /// Gets the barcode data.
        /// </summary>
        public string BarcodeData { get => _data; }
    }
}
