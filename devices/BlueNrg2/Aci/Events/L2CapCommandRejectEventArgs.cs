// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing L2CapCommandRejectEventArgs.
    /// </summary>
    public class L2CapCommandRejectEventArgs : EventArgs
    {
        /// <summary>
        /// Connection handle referring to the COS Channel where
        /// the Disconnection has been received.
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// This is the identifier which associate the request to the response.
        /// </summary>
        public readonly byte Identifier;

        /// <summary>
        /// Reason.
        /// </summary>
        public readonly ushort Reason;

        /// <summary>
        /// Length of following data.
        /// </summary>
        public readonly byte DataLength;

        /// <summary>
        /// Data field associated with Reason.
        /// </summary>
        public readonly byte[] Data;

        internal L2CapCommandRejectEventArgs(ushort connectionHandle, byte identifier, ushort reason, byte dataLength, byte[] data)
        {
            ConnectionHandle = connectionHandle;
            Identifier = identifier;
            Reason = reason;
            DataLength = dataLength;
            Data = data;
        }
    }
}
