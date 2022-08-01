// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing L2CapProcessTimeoutEventArgs.
    /// </summary>
    public class L2CapProcessTimeoutEventArgs : EventArgs
    {
        /// <summary>
        /// Handle of the connection related to this L2CAP procedure.
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// Length of following data.
        /// </summary>
        public readonly byte DataLength;

        /// <summary>
        /// Data.
        /// </summary>
        public readonly byte[] Data;

        internal L2CapProcessTimeoutEventArgs(ushort connectionHandle, byte dataLength, byte[] data)
        {
            ConnectionHandle = connectionHandle;
            DataLength = dataLength;
            Data = data;
        }
    }
}
