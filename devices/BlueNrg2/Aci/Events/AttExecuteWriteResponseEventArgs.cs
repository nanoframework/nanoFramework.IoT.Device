// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing AttExecuteWriteResponseEventArgs
    /// </summary>
    public class AttExecuteWriteResponseEventArgs : EventArgs
    {
        /// <summary>
        /// Connection handle related to the response.
        /// </summary>
        public readonly ushort ConnectionHandle;

        internal AttExecuteWriteResponseEventArgs(ushort connectionHandle)
        {
            ConnectionHandle = connectionHandle;
        }
    }
}
