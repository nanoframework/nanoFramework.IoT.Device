// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing HalScanRequestReportEventArgs.
    /// </summary>
    public class HalScanRequestReportEventArgs : EventArgs
    {
        /// <summary>
        /// N Size: 1 Octet (signed integer) Units: dBm.
        /// </summary>
        public readonly sbyte Rssi;

        /// <summary>
        /// <list type="bullet">
        /// <item><see cref="AddressType.PublicDeviceAddress"/></item>
        /// <item><see cref="AddressType.RandomDeviceAddress"/></item>
        /// <item><see cref="AddressType.PublicIdentityAddress"/> (Corresponds to Resolved Private Address)</item>
        /// <item><see cref="AddressType.RandomIdentityAddress"/> (Corresponds to Resolved Private Address)</item>
        /// </list>
        /// </summary>
        public readonly AddressType PeerAddressType;

        /// <summary>
        /// <see cref="AddressType.PublicDeviceAddress"/> or <see cref="AddressType.RandomDeviceAddress"/> of the peer device.
        /// </summary>
        public readonly byte[] PeerAddress;

        internal HalScanRequestReportEventArgs(sbyte rssi, AddressType peerAddressType, byte[] peerAddress)
        {
            Rssi = rssi;
            PeerAddressType = peerAddressType;
            PeerAddress = peerAddress;
        }
    }
}
