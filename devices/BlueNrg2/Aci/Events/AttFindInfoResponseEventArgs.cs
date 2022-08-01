// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing AttFindInfoResponseEventArgs.
    /// </summary>
    public class AttFindInfoResponseEventArgs : EventArgs
    {
        /// <summary>
        /// Connection handle related to the response. 
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// Format of the hanndle-uuid pairs.
        /// </summary>
        public readonly byte Format;

        /// <summary>
        /// Length of Handle_UUID_Pair in octets
        /// </summary>
        public readonly byte EventDataLength;

        /// <summary>
        /// A sequence of handle-uuid pairs. if format=1, each
        /// pair is:[2 octets for handle, 2 octets for UUIDs], if format=2, each
        /// pair is:[2 octets for handle, 16 octets for UUIDs]
        /// </summary>
        public readonly byte[] UuidPairHandle;

        internal AttFindInfoResponseEventArgs(ushort connectionHandle, byte format, byte eventDataLength, byte[] uuidPairHandle)
        {
            ConnectionHandle = connectionHandle;
            Format = format;
            EventDataLength = eventDataLength;
            UuidPairHandle = uuidPairHandle;
        }
    }
}
