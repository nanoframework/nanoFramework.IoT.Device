// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing GattPrepareWritePermitRequestEventArgs.
    /// </summary>
    public class GattPrepareWritePermitRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Handle of the connection on which there was the
        /// request to write the attribute.
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// The handle of the attribute.
        /// </summary>
        public readonly ushort AttributeHandle;

        /// <summary>
        /// The offset from which the prepare write has been requested.
        /// </summary>
        public readonly ushort Offset;

        /// <summary>
        /// Length of Data field.
        /// </summary>
        public readonly byte DataLength;

        /// <summary>
        /// The data that the client has requested to write.
        /// </summary>
        public readonly byte[] Data;

        internal GattPrepareWritePermitRequestEventArgs(
            ushort connectionHandle,
            ushort attributeHandle,
            ushort offset,
            byte dataLength,
            byte[] data)
        {
            ConnectionHandle = connectionHandle;
            AttributeHandle = attributeHandle;
            Offset = offset;
            DataLength = dataLength;
            Data = data;
        }
    }
}
