// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// Struct for connecting <see cref="NumberOfCompletedPackets"/> with their corresponding <see cref="ConnectionHandle"/>.
    /// </summary>
    public struct HandlePacketPair
    {
        /// <summary>
        /// Connection handle.
        /// </summary>
        public ushort ConnectionHandle;

        /// <summary>
        /// The number of HCI Data Packets that have been completed (transmitted or flushed)
        /// for the associated <see cref="ConnectionHandle"/> since the previous time the event was returned. 
        /// </summary>
        public ushort NumberOfCompletedPackets;
    }
}
