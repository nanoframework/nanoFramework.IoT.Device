// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing AttReadByTypeResponseEventArgs.
    /// </summary>
    public class AttReadByTypeResponseEventArgs : EventArgs
    {
        /// <summary>
        /// Connection handle related to the response.
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// The size of each attribute handle-value pair.
        /// </summary>
        public readonly byte HandleValuePairLenght;

        /// <summary>
        /// Length of Handle_Value_Pair_Data in octets
        /// </summary>
        public readonly byte DataLenght;

        /// <summary>
        /// Attribute Data List as defined in Bluetooth
        /// Core v4.1 spec. A sequence of handle-value pairs: [2 octets for
        /// Attribute Handle, (Handle_Value_Pair_Length - 2 octets) for Attribute
        /// Value]
        /// </summary>
        public readonly byte[] HandleValuePairData;

        internal AttReadByTypeResponseEventArgs(ushort connectionHandle, byte handleValuePairLenght, byte dataLenght, byte[] handleValuePairData)
        {
            ConnectionHandle = connectionHandle;
            HandleValuePairLenght = handleValuePairLenght;
            DataLenght = dataLenght;
            HandleValuePairData = handleValuePairData;
        }
    }
}
