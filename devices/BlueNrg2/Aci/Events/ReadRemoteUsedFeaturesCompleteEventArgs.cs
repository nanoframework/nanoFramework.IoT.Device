// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing ReadRemoteUsedFeaturesCompleteEventArgs.
    /// </summary>
    public class ReadRemoteUsedFeaturesCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// For standard error codes see Bluetooth specification, Vol. 2, part D. For proprietary error code refer to Error codes section.
        /// </summary>
        public readonly byte Status;

        /// <summary>
        /// Connection handle to be used to identify the connection with the peer device.
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// Bit Mask List of used LE features. For details see LE Link Layer specification.
        /// </summary>
        public readonly byte[] Features;

        internal ReadRemoteUsedFeaturesCompleteEventArgs(byte status, ushort connectionHandle, byte[] features)
        {
            Status = status;
            ConnectionHandle = connectionHandle;
            Features = features;
        }
    }
}
