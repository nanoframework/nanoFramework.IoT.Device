// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing L2CapConnectionUpdateRequestEventArgs.
    /// </summary>
    public class L2CapConnectionUpdateRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Handle of the connection related to this L2CAP procedure.
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// This is the identifier which associate the request to the response.
        /// </summary>
        public readonly byte Identifier;

        /// <summary>
        /// Length of the L2CAP connection update request.
        /// </summary>
        public readonly ushort L2CapLength;

        /// <summary>
        /// Minimum value for the connection event interval. This shall be
        /// less than or equal to <see cref="IntervalMax"/>. Time = N * 1.25 ms.
        /// </summary>
        public readonly ushort IntervalMin;

        /// <summary>
        /// Maximum value for the connection event interval. This shall be
        /// greater than or equal to <see cref="IntervalMin"/>. Time = N * 1.25 ms.
        /// </summary>
        public readonly ushort IntervalMax;

        /// <summary>
        /// Slave latency for the connection in number of connection events.
        /// </summary>
        public readonly ushort SlaveLatency;

        /// <summary>
        /// Defines connection timeout parameter in the following manner: Timeout Multiplier * 10ms.
        /// </summary>
        public readonly ushort TimeoutMultiplier;

        internal L2CapConnectionUpdateRequestEventArgs(
            ushort connectionHandle,
            byte identifier,
            ushort l2CapLength,
            ushort intervalMin,
            ushort intervalMax,
            ushort slaveLatency,
            ushort timeoutMultiplier)
        {
            ConnectionHandle = connectionHandle;
            Identifier = identifier;
            L2CapLength = l2CapLength;
            IntervalMin = intervalMin;
            IntervalMax = intervalMax;
            SlaveLatency = slaveLatency;
            TimeoutMultiplier = timeoutMultiplier;
        }
    }
}
