// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing ConnectionUpdateCompleteEventArgs.
    /// </summary>
    public class ConnectionUpdateCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// For standard error codes see Bluetooth specification, Vol. 2,
        /// part D. For proprietary error code refer to Error codes section
        /// </summary>
        public readonly byte Status;

        /// <summary>
        /// Connection handle to be used to identify the connection with the peer device. 
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// Connection interval used on this connection. Time = N * 1.25 ms
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

        internal ConnectionUpdateCompleteEventArgs(
            byte status,
            ushort connectionHandle,
            ushort connectionInterval,
            ushort connectionLatency,
            ushort supervisionTimeout)
        {
            Status = status;
            ConnectionHandle = connectionHandle;
            ConnectionInterval = connectionInterval;
            ConnectionLatency = connectionLatency;
            SupervisionTimeout = supervisionTimeout;
        }
    }
}
