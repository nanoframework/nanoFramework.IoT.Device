// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing EnhancedConnectionCompleteEventArgs.
    /// </summary>
    public class EnhancedConnectionCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// For standard error codes see Bluetooth specification, Vol. 2,
        /// part D. For proprietary error code refer to Error codes section.
        /// </summary>
        public readonly byte Status;

        /// <summary>
        /// Connection handle to be used to identify the
        /// connection with the peer device.
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// Role of the local device in the connection.
        /// </summary>
        public readonly DeviceRole Role;

        /// <summary>
        /// Public Device Address, Random Device Address, Public Identity Address
        /// (Corresponds to Resolved Private Address), Random (Static) Identity
        /// Address (Corresponds to Resolved Private Address).
        /// </summary>
        public readonly AddressType PeerAddressType;

        /// <summary>
        /// Public Device Address, Random Device Address, Public
        /// Identity Address or Random (static) Identity Address of the device to
        /// be connected.
        /// </summary>
        public readonly byte[] PeerAddress;

        /// <summary>
        /// Resolvable Private Address being used
        /// by the local device for this connection. This is only valid when the
        /// Own_Address_Type is set to 0x02 or 0x03. For other Own_Address_Type
        /// values, the Controller shall return all zeros.
        /// </summary>
        public readonly byte[] LocalResolvablePrivateAddress;

        /// <summary>
        /// Resolvable Private Address being used
        /// by the peer device for this connection. This is only valid for
        /// Peer_Address_Type 0x02 and 0x03. For other Peer_Address_Type values,
        /// the Controller shall return all zeros.
        /// </summary>
        public readonly byte[] PeerResolvablePrivateAddress;

        /// <summary>
        /// Connection interval used on this connection. Time = N * 1.25 ms.
        /// </summary>
        public readonly ushort ConnectionInterval;

        /// <summary>
        /// Slave latency for the connection in number of connection events.
        /// </summary>
        public readonly ushort ConnectionLatency;

        /// <summary>
        /// Supervision timeout for the LE Link. It shall be a
        /// multiple of 10 ms and larger than (1 + connSlaveLatency) *
        /// connInterval * 2. Time = N * 10 ms.
        /// </summary>
        public readonly ushort SupervisionTimeout;

        /// <summary>
        /// Master clock accuracy. Only valid for a slave.
        /// </summary>
        public readonly ClockAccuracy MasterClockAccuracy;

        internal EnhancedConnectionCompleteEventArgs(
            byte status,
            ushort connectionHandle,
            DeviceRole role,
            AddressType peerAddressType,
            byte[] peerAddress,
            byte[] localResolvablePrivateAddress,
            byte[] peerResolvablePrivateAddress,
            ushort connectionInterval,
            ushort connectionLatency,
            ushort supervisionTimeout,
            ClockAccuracy masterClockAccuracy)
        {
            Status = status;
            ConnectionHandle = connectionHandle;
            Role = role;
            PeerAddressType = peerAddressType;
            PeerAddress = peerAddress;
            LocalResolvablePrivateAddress = localResolvablePrivateAddress;
            PeerResolvablePrivateAddress = peerResolvablePrivateAddress;
            ConnectionInterval = connectionInterval;
            ConnectionLatency = connectionLatency;
            SupervisionTimeout = supervisionTimeout;
            MasterClockAccuracy = masterClockAccuracy;
        }
    }
}
