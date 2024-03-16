// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// enum holding types of advertising.
    /// </summary>
    public enum AdvertisingType : byte
    {
        /// <summary>
        /// Connectable undirected advertising.
        /// </summary>
        ConnectableUndirected = 0x00,

        /// <summary>
        /// Connectable directed advertising.
        /// </summary>
        ConnectableDirected = 0x01,

        /// <summary>
        /// Scannable undirected advertising.
        /// </summary>
        ScannableUndirected = 0x02,

        /// <summary>
        /// Non connectable undirected advertising.
        /// </summary>
        NonConnectableUndirected = 0x03,

        /// <summary>
        /// Scan response.
        /// </summary>
        ScanResponse = 0x04
    }
}
