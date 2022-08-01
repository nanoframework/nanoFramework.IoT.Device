// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing GattReadPermitRequestEventArgs.
    /// </summary>
    public class GattReadPermitRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Connection handle related to the response.
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// The handle of the attribute.
        /// </summary>
        public readonly ushort AttributeHandle;

        /// <summary>
        /// Contains the offset from which the read has been requested.
        /// </summary>
        public readonly ushort Offset;

        internal GattReadPermitRequestEventArgs(ushort connectionHandle, ushort attributeHandle, ushort offset)
        {
            ConnectionHandle = connectionHandle;
            AttributeHandle = attributeHandle;
            Offset = offset;
        }
    }
}
