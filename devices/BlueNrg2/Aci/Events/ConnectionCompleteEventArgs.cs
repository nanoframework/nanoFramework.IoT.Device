// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing ConnectionCompleteEventArgs.
    /// </summary>
    public class ConnectionCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// For standard error codes see Bluetooth specification, Vol. 2,
        /// part D. For proprietary error code refer to Error codes section
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
        /// The address type of the peer device.
        /// </summary>
        public readonly AddressType PeerAddressType;

        /// <summary>
        /// Public Device Address or Random Device Address of the peer device.
        /// </summary>
        public readonly byte[] PeerAddress;

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

        internal ConnectionCompleteEventArgs(
            byte status,
            ushort connectionHandle,
            DeviceRole role,
            AddressType peerAddressType,
            byte[] peerAddress,
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
            ConnectionInterval = connectionInterval;
            ConnectionLatency = connectionLatency;
            SupervisionTimeout = supervisionTimeout;
            MasterClockAccuracy = masterClockAccuracy;
        }
    }
}
