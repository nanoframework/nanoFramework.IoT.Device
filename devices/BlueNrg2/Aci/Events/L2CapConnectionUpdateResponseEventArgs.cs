// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing L2CapConnectionUpdateResponseEventArgs.
    /// </summary>
    public class L2CapConnectionUpdateResponseEventArgs : EventArgs
    {
        /// <summary>
        /// Connection handle referring to the COS Channel where the Disconnection has been received.
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// Result.
        /// </summary>
        public readonly ushort Result;

        internal L2CapConnectionUpdateResponseEventArgs(ushort connectionHandle, ushort result)
        {
            ConnectionHandle = connectionHandle;
            Result = result;
        }
    }
}
