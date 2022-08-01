// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing DataLengthChangeEventArgs.
    /// </summary>
    public class DataLengthChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Connection_Handle to be used to identify a connection. 
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// The maximum number of payload octets in a Link Layer Data
        /// Channel PDU that the local Controller will send on this connection
        /// (connEffectiveMaxTxOctets defined in [Vol 6] Part B, Section 4.5.10).
        /// Range 0x001B-0x00FB (0x0000 - 0x001A and 0x00FC - 0xFFFF Reserved for
        /// future use) 
        /// </summary>
        public readonly ushort MaxTxOctets;

        /// <summary>
        /// The maximum time that the local Controller will take to send
        /// a Link Layer Data Channel PDU on this connection (connEffectiveMaxTx-
        /// Time defined in [Vol 6] Part B, Section 4.5.10). Range 0x0148-0x0848
        /// (0x0000 - 0x0127 and 0x0849 - 0xFFFF Reserved for future use)
        /// </summary>
        public readonly ushort MaxTxTime;

        /// <summary>
        /// The maximum number of payload octets in a Link Layer Data
        /// Channel PDU that the local controller expects to receive on this
        /// connection (connEfectiveMaxRxOctets defined in [Vol 6] Part B, Section
        /// 4.5.10). Range 0x001B-0x00FB (0x0000 - 0x001A and 0x00FC - 0xFFFF
        /// Reserved for future use)
        /// </summary>
        public readonly ushort MaxRxOctets;

        /// <summary>
        /// The maximum time that the local Controller expects to take
        /// to receive a Link Layer Data Channel PDU on this connection
        /// (connEffectiveMax-RxTime defined in [Vol 6] Part B, Section 4.5.10).
        /// Range 0x0148-0x0848 (0x0000 - 0x0127 and 0x0849 - 0xFFFF Reserved for
        /// future use)
        /// </summary>
        public readonly ushort MaxRxTime;

        internal DataLengthChangeEventArgs(ushort connectionHandle, ushort maxTxOctets, ushort maxTxTime, ushort maxRxOctets, ushort maxRxTime)
        {
            ConnectionHandle = connectionHandle;
            MaxTxOctets = maxTxOctets;
            MaxTxTime = maxTxTime;
            MaxRxOctets = maxRxOctets;
            MaxRxTime = maxRxTime;
        }
    }
}
