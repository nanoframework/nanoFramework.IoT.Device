// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing GattAttributeModifiedEventArgs.
    /// </summary>
    public class GattAttributeModifiedEventArgs : EventArgs
    {
        /// <summary>
        /// The connection handle which modified the attribute.
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// Handle of the attribute that was modified.
        /// </summary>
        public readonly ushort AttributeHandle;

        /// <summary>
        /// SoC mode: the offset is never used and it is always 0.
        /// <list type="bullet">
        ///     <listheader>
        ///         <term>Network coprocessor mode:</term>
        ///     </listheader>
        ///     <item>
        ///         Bits 0-14: offset of the reported value inside the attribute.
        ///     </item>
        ///     <item>
        ///         Bit 15: if the entire value of the attribute does not fit inside a single <see cref="EventProcessor.GattAttributeModifiedEvent"/> event,
        ///         this bit is set to 1 to notify that other <see cref="EventProcessor.GattAttributeModifiedEvent"/> events will follow to report the remaining value.
        ///     </item>
        /// </list>
        /// </summary>
        public readonly ushort Offset;

        /// <summary>
        /// Length of Attr_Data in octets.
        /// </summary>
        public readonly ushort AttributeDataLength;

        /// <summary>
        /// The modified value.
        /// </summary>
        public readonly byte[] AttributeData;

        internal GattAttributeModifiedEventArgs(
            ushort connectionHandle,
            ushort attributeHandle,
            ushort offset,
            ushort attributeDataLength,
            byte[] attributeData)
        {
            ConnectionHandle = connectionHandle;
            AttributeHandle = attributeHandle;
            Offset = offset;
            AttributeDataLength = attributeDataLength;
            AttributeData = attributeData;
        }
    }
}
