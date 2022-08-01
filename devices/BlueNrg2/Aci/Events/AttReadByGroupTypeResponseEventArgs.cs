// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// class containing AttReadByGroupTypeResponseEventArgs.
    /// </summary>
    public class AttReadByGroupTypeResponseEventArgs : EventArgs
    {
        /// <summary>
        /// Connection handle related to the response.
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// The size of each attribute data.
        /// </summary>
        public readonly byte AttributeDataLength;

        /// <summary>
        /// Length of Attribute_Data_List in octets.
        /// </summary>
        public readonly byte DataLength;

        /// <summary>
        /// Attribute Data List as defined in Bluetooth Core
        /// v4.1 spec. A sequence of attribute handle, end group handle, attribute
        /// value tuples: [2 octets for Attribute Handle, 2 octets End Group
        /// Handle, (Attribute_Data_Length - 4 octets) for Attribute Value]
        /// </summary>
        public readonly byte[] AttributeDataList;

        internal AttReadByGroupTypeResponseEventArgs(ushort connectionHandle, byte attributeDataLength, byte dataLength, byte[] attributeDataList)
        {
            ConnectionHandle = connectionHandle;
            AttributeDataLength = attributeDataLength;
            DataLength = dataLength;
            AttributeDataList = attributeDataList;
        }
    }
}
