// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.AtomQrCode
{
    /// <summary>
    /// Represents the method that will handle the Iot.Device.AtomQrCode.BarcodeDataAvailable event of a <see cref="QrCodeReader"/> object.
    /// </summary>
    /// <param name="sender">The sender of the event, which is the <see cref="QrCodeReader"/> object.</param>
    /// <param name="e">A <see cref="BarcodeDataAvailableEventArgs"/> object that contains the event data.</param>
    public delegate void BarcodeDataAvailableEventHandler(
        object sender,
        BarcodeDataAvailableEventArgs e);
}
