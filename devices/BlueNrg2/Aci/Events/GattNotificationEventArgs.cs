// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing GattNotificationEventArgs.
    /// </summary>
    public class GattNotificationEventArgs : EventArgs
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
        /// Length of <see cref="AttributeValue"/> in octets.
        /// </summary>
        public readonly byte AttributeValueLength;

        /// <summary>
        /// The current value of the attribute.
        /// </summary>
        public readonly byte[] AttributeValue;

        internal GattNotificationEventArgs(ushort connectionHandle, ushort attributeHandle, byte attributeValueLength, byte[] attributeValue)
        {
            ConnectionHandle = connectionHandle;
            AttributeHandle = attributeHandle;
            AttributeValueLength = attributeValueLength;
            AttributeValue = attributeValue;
        }
    }
}
