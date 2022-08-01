// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// class containing AttReadMultipleResponseEventArgs.
    /// </summary>
    public class AttReadMultipleResponseEventArgs : EventArgs
    {
        /// <summary>
        /// Connection handle related to the response.
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// Length of the following data.
        /// </summary>
        public readonly byte EventDataLength;

        /// <summary>
        /// A set of two or more values. A concatenation of
        /// attribute values for each of the attribute handles in the request in
        /// the order that they were requested.
        /// </summary>
        public readonly byte[] SetOfValues;

        internal AttReadMultipleResponseEventArgs(ushort connectionHandle, byte eventDataLength, byte[] setOfValues)
        {
            ConnectionHandle = connectionHandle;
            EventDataLength = eventDataLength;
            SetOfValues = setOfValues;
        }
    }
}
