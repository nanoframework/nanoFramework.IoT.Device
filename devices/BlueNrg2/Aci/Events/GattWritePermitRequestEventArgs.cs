// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing GattWritePermitRequestEventArgs.
    /// </summary>
    public class GattWritePermitRequestEventArgs : EventArgs
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
        /// Length of Data field.
        /// </summary>
        public readonly byte DataLength;

        /// <summary>
        /// The data that the client has requested to write.
        /// </summary>
        public readonly byte[] Data;

        internal GattWritePermitRequestEventArgs(ushort connectionHandle, ushort attributeHandle, byte dataLength, byte[] data)
        {
            ConnectionHandle = connectionHandle;
            AttributeHandle = attributeHandle;
            DataLength = dataLength;
            Data = data;
        }
    }
}
