// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing GattDiscoverReadCharacteristicByUuidResponseEventArgs.
    /// </summary>
    public class GattDiscoverReadCharacteristicByUuidResponseEventArgs : EventArgs
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
        /// The attribute value will be a service declaration as
        /// defined in Bluetooth Core v4.0 spec  (vol.3, Part G, ch. 3.3.1), when
        /// a "Discover Characteristics By UUID" has been started.  It will be the
        /// value of the Characteristic if a "Read using Characteristic UUID" has
        /// been performed.
        /// </summary>
        public readonly byte[] AttributeValue;

        internal GattDiscoverReadCharacteristicByUuidResponseEventArgs(
            ushort connectionHandle,
            ushort attributeHandle,
            byte attributeValueLength,
            byte[] attributeValue)
        {
            ConnectionHandle = connectionHandle;
            AttributeHandle = attributeHandle;
            AttributeValueLength = attributeValueLength;
            AttributeValue = attributeValue;
        }
    }
}
