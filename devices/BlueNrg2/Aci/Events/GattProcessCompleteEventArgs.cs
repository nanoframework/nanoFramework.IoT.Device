// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing GattProcessCompleteEventArgs.
    /// </summary>
    public class GattProcessCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// Connection handle related to the response.
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// Indicates whether the procedure completed with an error or was successful.
        /// </summary>
        public readonly BleStatus ErrorCode;

        internal GattProcessCompleteEventArgs(ushort connectionHandle, BleStatus errorCode)
        {
            ConnectionHandle = connectionHandle;
            ErrorCode = errorCode;
        }
    }
}
