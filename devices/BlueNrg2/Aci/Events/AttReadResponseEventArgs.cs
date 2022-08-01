// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing AttReadResponseEventArgs.
    /// </summary>
    public class AttReadResponseEventArgs : EventArgs
    {
        /// <summary>
        /// Connection handle related to the response.
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// Length of following data.
        /// </summary>
        public readonly byte EventDataLenght;

        /// <summary>
        /// The value of the attribute.
        /// </summary>
        public readonly byte[] AttributeValue;

        internal AttReadResponseEventArgs(ushort connectionHandle, byte eventDataLenght, byte[] attributeValue)
        {
            ConnectionHandle = connectionHandle;
            EventDataLenght = eventDataLenght;
            AttributeValue = attributeValue;
        }
    }
}
