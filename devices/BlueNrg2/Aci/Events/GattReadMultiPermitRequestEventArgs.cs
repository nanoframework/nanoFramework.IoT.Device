// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing GattReadMultiPermitRequestEventArgs.
    /// </summary>
    public class GattReadMultiPermitRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Handle of the connection which requested to read the attribute.
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// Number of handles.
        /// </summary>
        public readonly byte NumberOfHandles;

        /// <summary>
        /// Array of handles.
        /// </summary>
        public readonly ushort[] Handles;

        internal GattReadMultiPermitRequestEventArgs(ushort connectionHandle, byte numberOfHandles, ushort[] handles)
        {
            ConnectionHandle = connectionHandle;
            NumberOfHandles = numberOfHandles;
            Handles = handles;
        }
    }
}
