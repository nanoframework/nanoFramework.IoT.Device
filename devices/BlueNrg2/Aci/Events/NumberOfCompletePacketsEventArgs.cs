// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing NumberOfCompletePacketsEventArgs.
    /// </summary>
    public class NumberOfCompletePacketsEventArgs : EventArgs
    {
        /// <summary>
        /// The number of <see cref="HandlePacketPair.ConnectionHandle"/> and
        /// <see cref="HandlePacketPair.NumberOfCompletedPackets"/> parameters pairs contained in this event
        /// </summary>
        public readonly byte NumberOfHandles;

        /// <summary>
        /// See <see cref="HandlePacketPair"/>.
        /// </summary>
        public readonly HandlePacketPair[] HandlePacketPairs;

        internal NumberOfCompletePacketsEventArgs(byte numberOfHandles, HandlePacketPair[] handlePacketPairs)
        {
            NumberOfHandles = numberOfHandles;
            HandlePacketPairs = handlePacketPairs;
        }
    }
}
