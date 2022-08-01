// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing AttExchangeMtuResponseEventArgs
    /// </summary>
    public class AttExchangeMtuResponseEventArgs : EventArgs
    {
        /// <summary>
        /// Connection handle related to the response.
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// ATT_MTU value agreed between server and client.
        /// </summary>
        public readonly ushort ServerRxMtu;

        internal AttExchangeMtuResponseEventArgs(ushort connectionHandle, ushort serverRxMtu)
        {
            ConnectionHandle = connectionHandle;
            ServerRxMtu = serverRxMtu;
        }
    }
}
