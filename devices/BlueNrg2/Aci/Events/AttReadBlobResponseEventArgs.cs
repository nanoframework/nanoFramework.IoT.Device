// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing AttReadBlobResponseEventArgs.
    /// </summary>
    public class AttReadBlobResponseEventArgs : EventArgs
    {
        /// <summary>
        /// Connection handle related to the response.
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// Length of following data.
        /// </summary>
        public readonly byte EventDataLength;

        /// <summary>
        /// Part of the attribute value.
        /// </summary>
        public readonly byte[] AttributeValue;

        internal AttReadBlobResponseEventArgs(ushort connectionHandle, byte eventDataLength, byte[] attributeValue)
        {
            ConnectionHandle = connectionHandle;
            EventDataLength = eventDataLength;
            AttributeValue = attributeValue;
        }
    }
}
