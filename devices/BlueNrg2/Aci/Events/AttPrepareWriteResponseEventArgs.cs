// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing AttPrepareWriteResponseEventArgs.
    /// </summary>
    public class AttPrepareWriteResponseEventArgs : EventArgs
    {
        /// <summary>
        /// Connection handle related to the response.
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// The handle of the attribute to be written.
        /// </summary>
        public readonly ushort AttributeHandle;

        /// <summary>
        /// The offset of the first octet to be written.
        /// </summary>
        public readonly ushort Offset;

        /// <summary>
        /// Length of Part_Attribute_Value in octets.
        /// </summary>
        public readonly byte PartAttributeValueLength;

        /// <summary>
        /// The value of the attribute to be written.
        /// </summary>
        public readonly byte[] PartAttributeValue;

        internal AttPrepareWriteResponseEventArgs(
            ushort connectionHandle,
            ushort attributeHandle,
            ushort offset,
            byte partAttributeValueLength,
            byte[] partAttributeValue)
        {
            ConnectionHandle = connectionHandle;
            AttributeHandle = attributeHandle;
            Offset = offset;
            PartAttributeValueLength = partAttributeValueLength;
            PartAttributeValue = partAttributeValue;
        }
    }
}
